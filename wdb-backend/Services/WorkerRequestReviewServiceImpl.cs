using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

/// <summary>
/// Worker-side request review service.
/// Supports:
/// - list pending requests
/// - approve/reject requested fields
/// - approve/reject custom request by adding a new worker_info item
///
/// Blockchain design:
/// One Submit Review action writes one RequestReviewed blockchain record.
/// The record contains the whole request review summary instead of one record per category.
/// </summary>
public class WorkerRequestReviewServiceImpl : IWorkerRequestReviewService
{
    private readonly AppDbContext _context;
    private readonly IBlockchainService _blockchainService;
    private readonly ILogger<WorkerRequestReviewServiceImpl> _logger;

    public WorkerRequestReviewServiceImpl(
        AppDbContext context,
        IBlockchainService blockchainService,
        ILogger<WorkerRequestReviewServiceImpl> logger)
    {
        _context = context;
        _blockchainService = blockchainService;
        _logger = logger;
    }

    public async Task<List<WorkerActiveRequestDto>> GetActiveRequestsAsync(
        Guid workerId,
        CancellationToken cancellationToken = default)
    {
        var requests = await _context.Requests
            .Where(r =>
                r.WorkerId == workerId &&
                (
                    r.Permissions.Any(p => p.Status == PermissionStatus.Pending) ||
                    r.CustomRequestStatus == "pending"
                ))
            .Include(r => r.Permissions)
                .ThenInclude(p => p.Field)
                    .ThenInclude(f => f!.Category)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo)
                    .ThenInclude(wi => wi!.Field)
                        .ThenInclude(f => f!.Category)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var workerInfos = await _context.WorkerInfos
            .Where(w => w.WorkerId == workerId)
            .Include(w => w.Field)
                .ThenInclude(f => f!.Category)
            .ToListAsync(cancellationToken);

        var workerInfoByFieldId = workerInfos
            .Where(w => w.FieldId.HasValue)
            .ToDictionary(w => w.FieldId!.Value, w => w);

        var result = new List<WorkerActiveRequestDto>();

        foreach (var request in requests)
        {
            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.Id == request.EmployerId, cancellationToken);

            var dto = new WorkerActiveRequestDto
            {
                RequestId = request.Id,
                EmployerId = request.EmployerId,
                CompanyName = employer?.Name ?? "Unknown",
                Reason = request.Reason,
                ExpiryDate = request.ExpiryDate,
                CreatedAt = request.CreatedAt
            };

            if (!string.IsNullOrWhiteSpace(request.CustomRequest) &&
                request.CustomRequestStatus == "pending")
            {
                dto.CustomRequest = new WorkerCustomRequestDto
                {
                    Description = request.CustomRequest,
                    Status = request.CustomRequestStatus
                };
            }

            var pendingPermissions = request.Permissions
                .Where(p => p.Status == PermissionStatus.Pending)
                .OrderBy(p => ResolveCategory(p, workerInfoByFieldId))
                .ThenBy(p => ResolveLabel(p, workerInfoByFieldId))
                .ToList();

            foreach (var permission in pendingPermissions)
            {
                var info = ResolveWorkerInfoForDisplay(permission, workerInfoByFieldId);

                var label = ResolveLabel(permission, workerInfoByFieldId);
                var category = ResolveCategory(permission, workerInfoByFieldId);
                var type = ResolveType(permission, workerInfoByFieldId);

                var hasValue = !string.IsNullOrWhiteSpace(info?.Value);
                var canApprove = info != null && hasValue;

                dto.Items.Add(new WorkerRequestReviewItemDto
                {
                    PermissionId = permission.Id,
                    FieldId = permission.FieldId ?? info?.FieldId,
                    InfoId = permission.InfoId ?? info?.Id,
                    Label = label,
                    Category = category,
                    Type = type,
                    Value = info?.Value,
                    Status = permission.Status,
                    HasValue = hasValue,
                    CanApprove = canApprove,
                    CannotApproveReason = canApprove
                        ? null
                        : "This field has no saved value. Please fill it in before approving."
                });
            }

            result.Add(dto);
        }

        return result;
    }

    public async Task SubmitReviewAsync(
        Guid workerId,
        Guid requestId,
        SubmitWorkerRequestReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var hasItemReviews = request.Items.Count > 0;
        var hasCustomDecision = request.CustomRequestDecision != null;

        if (!hasItemReviews && !hasCustomDecision)
            throw new InvalidOperationException("NO_REVIEW_ACTIONS");

        var dataRequest = await _context.Requests
            .Include(r => r.Permissions)
                .ThenInclude(p => p.Field)
                    .ThenInclude(f => f!.Category)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo)
                    .ThenInclude(wi => wi!.Field)
                        .ThenInclude(f => f!.Category)
            .FirstOrDefaultAsync(
                r => r.Id == requestId && r.WorkerId == workerId,
                cancellationToken)
            ?? throw new KeyNotFoundException("REQUEST_NOT_FOUND");

        var hasApprovedDecision = HasApprovedDecision(request);

        if (hasApprovedDecision)
        {
            if (!request.ExpiryDate.HasValue)
                throw new InvalidOperationException("EXPIRY_DATE_REQUIRED");

            if (request.ExpiryDate.Value <= DateTime.UtcNow)
                throw new InvalidOperationException("INVALID_EXPIRY_DATE");

            dataRequest.ExpiryDate = request.ExpiryDate.Value.ToUniversalTime();
        }
        else
        {
            dataRequest.ExpiryDate = null;
        }

        await ReviewPermissionItemsAsync(
            workerId,
            dataRequest,
            request.Items,
            cancellationToken);

        Guid? customPermissionId = null;
        string? customDecision = null;
        string? customRequestDescription = null;

        if (request.CustomRequestDecision != null)
        {
            customDecision = request.CustomRequestDecision.Decision.Trim().ToLowerInvariant();
            customRequestDescription = dataRequest.CustomRequest;

            customPermissionId = await ReviewCustomRequestAsync(
                workerId,
                dataRequest,
                request.CustomRequestDecision,
                cancellationToken);
        }

        _context.Notifications.Add(new wdb_backend.Models.Notification
        {
            RecipientWorkerId = null,
            RecipientEmployerId = dataRequest.EmployerId,
            Type = "REQUEST_REVIEWED",
            RequestId = dataRequest.Id,
            IsRead = false
        });

        await _context.SaveChangesAsync(cancellationToken);

        await LogRequestReviewToBlockchainAsync(
            workerId,
            dataRequest.Id,
            request.Items.Select(i => i.PermissionId).ToList(),
            customPermissionId,
            customDecision,
            customRequestDescription,
            dataRequest.ExpiryDate,
            cancellationToken);
    }

    private static bool HasApprovedDecision(SubmitWorkerRequestReviewRequest request)
    {
        var hasApprovedItem = request.Items.Any(i =>
            i.Decision.Trim().Equals("approved", StringComparison.OrdinalIgnoreCase));

        var hasApprovedCustomRequest =
            request.CustomRequestDecision != null &&
            request.CustomRequestDecision.Decision.Trim()
                .Equals("approved", StringComparison.OrdinalIgnoreCase);

        return hasApprovedItem || hasApprovedCustomRequest;
    }

    private async Task ReviewPermissionItemsAsync(
        Guid workerId,
        Request dataRequest,
        List<SubmitWorkerRequestReviewItem> items,
        CancellationToken cancellationToken)
    {
        var duplicatedPermissionIds = items
            .GroupBy(i => i.PermissionId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatedPermissionIds.Any())
            throw new InvalidOperationException("DUPLICATE_REVIEW_ITEM");

        foreach (var item in items)
        {
            var permission = dataRequest.Permissions
                .FirstOrDefault(p => p.Id == item.PermissionId)
                ?? throw new KeyNotFoundException("PERMISSION_NOT_FOUND");

            if (permission.WorkerId != workerId)
                throw new UnauthorizedAccessException("PERMISSION_NOT_OWNED_BY_WORKER");

            if (permission.Status != PermissionStatus.Pending)
                throw new InvalidOperationException("PERMISSION_NOT_PENDING");

            var decision = item.Decision.Trim().ToLowerInvariant();

            if (decision == "approved")
            {
                var workerInfo = await ResolveWorkerInfoForApprovalAsync(
                    workerId,
                    permission,
                    cancellationToken);

                if (workerInfo == null || string.IsNullOrWhiteSpace(workerInfo.Value))
                    throw new InvalidOperationException("FIELD_VALUE_REQUIRED");

                permission.InfoId = workerInfo.Id;
                permission.Status = PermissionStatus.Approved;
                permission.LastUpdatedAt = DateTime.UtcNow;
            }
            else if (decision == "rejected")
            {
                permission.Status = PermissionStatus.Rejected;
                permission.LastUpdatedAt = DateTime.UtcNow;
            }
            else
            {
                throw new InvalidOperationException("INVALID_DECISION");
            }
        }
    }

    /// <summary>
    /// Reviews the custom request.
    /// Returns the new approved permission id when the worker adds and approves a custom item.
    /// Returns null when the custom request is rejected.
    /// </summary>
    private async Task<Guid?> ReviewCustomRequestAsync(
        Guid workerId,
        Request dataRequest,
        SubmitWorkerCustomRequestDecision decision,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dataRequest.CustomRequest) ||
            dataRequest.CustomRequestStatus != "pending")
        {
            throw new InvalidOperationException("CUSTOM_REQUEST_NOT_PENDING");
        }

        var normalizedDecision = decision.Decision.Trim().ToLowerInvariant();

        if (normalizedDecision == "rejected")
        {
            dataRequest.CustomRequestStatus = "rejected";
            return null;
        }

        if (normalizedDecision != "approved")
            throw new InvalidOperationException("INVALID_CUSTOM_REQUEST_DECISION");

        var label = decision.Label?.Trim();
        var type = decision.Type?.Trim().ToLowerInvariant();
        var value = decision.Value?.Trim();

        if (string.IsNullOrWhiteSpace(label))
            throw new InvalidOperationException("CUSTOM_LABEL_REQUIRED");

        if (type != "text" && type != "file")
            throw new InvalidOperationException("INVALID_WORKER_INFO_TYPE");

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("CUSTOM_VALUE_REQUIRED");

        var duplicateExists = await _context.WorkerInfos
            .AnyAsync(
                w => w.WorkerId == workerId &&
                     w.CustomLabel != null &&
                     w.CustomLabel.ToLower() == label.ToLower(),
                cancellationToken);

        if (duplicateExists)
            throw new InvalidOperationException("CUSTOM_LABEL_EXISTS");

        var workerInfo = new WorkerInfo
        {
            Id = Guid.NewGuid(),
            WorkerId = workerId,
            FieldId = null,
            CustomLabel = label,
            Type = type,
            Value = value,
            UpdatedAt = DateTime.UtcNow
        };

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            RequestId = dataRequest.Id,
            WorkerId = workerId,
            FieldId = null,
            InfoId = workerInfo.Id,
            Status = PermissionStatus.Approved,
            LastUpdatedAt = DateTime.UtcNow
        };

        _context.WorkerInfos.Add(workerInfo);
        _context.Permissions.Add(permission);

        dataRequest.CustomRequestStatus = "approved";

        return permission.Id;
    }

    private async Task LogRequestReviewToBlockchainAsync(
        Guid workerId,
        Guid requestId,
        List<Guid> reviewedPermissionIds,
        Guid? customPermissionId,
        string? customDecision,
        string? customRequestDescription,
        DateTime? expiryDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var dataRequest = await _context.Requests
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.Id == requestId && r.WorkerId == workerId,
                    cancellationToken)
                ?? throw new KeyNotFoundException("REQUEST_NOT_FOUND_FOR_BLOCKCHAIN_LOG");

            var worker = await _context.Workers
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workerId, cancellationToken)
                ?? throw new KeyNotFoundException("WORKER_NOT_FOUND_FOR_BLOCKCHAIN_LOG");

            var employer = await _context.Employers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == dataRequest.EmployerId, cancellationToken)
                ?? throw new KeyNotFoundException("EMPLOYER_NOT_FOUND_FOR_BLOCKCHAIN_LOG");

            if (string.IsNullOrWhiteSpace(worker.PrivateKey))
                throw new InvalidOperationException("WORKER_PRIVATE_KEY_MISSING");

            if (string.IsNullOrWhiteSpace(worker.BlockchainAddress))
                throw new InvalidOperationException("WORKER_BLOCKCHAIN_ADDRESS_MISSING");

            if (string.IsNullOrWhiteSpace(employer.BlockchainAddress))
                throw new InvalidOperationException("EMPLOYER_BLOCKCHAIN_ADDRESS_MISSING");

            var allPermissionIdsToLog = reviewedPermissionIds
                .Where(id => id != Guid.Empty)
                .ToList();

            if (customPermissionId.HasValue && customPermissionId.Value != Guid.Empty)
            {
                allPermissionIdsToLog.Add(customPermissionId.Value);
            }

            var permissionsToLog = await _context.Permissions
                .AsNoTracking()
                .Where(p =>
                    p.RequestId == requestId &&
                    allPermissionIdsToLog.Contains(p.Id))
                .Include(p => p.Field)
                    .ThenInclude(f => f!.Category)
                .Include(p => p.WorkerInfo)
                    .ThenInclude(wi => wi!.Field)
                        .ThenInclude(f => f!.Category)
                .ToListAsync(cancellationToken);

            var permissionIds = string.Join(
                ",",
                permissionsToLog
                    .OrderBy(p => ResolveCategory(p))
                    .ThenBy(p => ResolveLabel(p))
                    .Select(p => p.Id.ToString()));

            var reviewSummary = BuildRequestReviewSummary(
                permissionsToLog,
                customDecision,
                customRequestDescription,
                customPermissionId,
                expiryDate);

            _logger.LogWarning(
                "Writing request-level blockchain review log. RequestId={RequestId}, PermissionCount={PermissionCount}, Summary={Summary}",
                requestId,
                permissionsToLog.Count,
                reviewSummary);

            var txHash = await _blockchainService.LogCategoryTransactionAsync(
                privateKey: worker.PrivateKey!,
                employerAddress: employer.BlockchainAddress!,
                workerAddress: worker.BlockchainAddress!,
                requestId: dataRequest.Id.ToString(),
                category: "RequestReview",
                permissionIds: permissionIds,
                itemLabels: reviewSummary,
                action: BlockchainAction.RequestReviewed,
                cancellationToken: cancellationToken);

            _logger.LogWarning(
                "Request-level blockchain review log written successfully. RequestId={RequestId}, TxHash={TxHash}",
                requestId,
                txHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "REQUEST REVIEW BLOCKCHAIN LOG FAILED. RequestId={RequestId}, Error={ErrorMessage}",
                requestId,
                ex.Message);
        }
    }

    private static string BuildRequestReviewSummary(
        List<Permission> permissions,
        string? customDecision,
        string? customRequestDescription,
        Guid? customPermissionId,
        DateTime? expiryDate)
    {
        var sections = new List<string>();

        if (expiryDate.HasValue)
        {
            sections.Add($"EXPIRY_DATE | {expiryDate.Value:O}");
        }

        var approvedGroups = permissions
            .Where(p => p.Status == PermissionStatus.Approved)
            .GroupBy(ResolveCategory)
            .OrderBy(g => g.Key)
            .ToList();

        if (approvedGroups.Any())
        {
            var approvedText = string.Join(
                "; ",
                approvedGroups.Select(g =>
                    $"{g.Key}: {string.Join(", ", g.OrderBy(ResolveLabel).Select(ResolveLabel))}"));

            sections.Add($"APPROVED | {approvedText}");
        }

        var rejectedGroups = permissions
            .Where(p => p.Status == PermissionStatus.Rejected)
            .GroupBy(ResolveCategory)
            .OrderBy(g => g.Key)
            .ToList();

        if (rejectedGroups.Any())
        {
            var rejectedText = string.Join(
                "; ",
                rejectedGroups.Select(g =>
                    $"{g.Key}: {string.Join(", ", g.OrderBy(ResolveLabel).Select(ResolveLabel))}"));

            sections.Add($"REJECTED | {rejectedText}");
        }

        if (!string.IsNullOrWhiteSpace(customDecision))
        {
            if (customDecision == "approved" && customPermissionId.HasValue)
            {
                var customPermission = permissions
                    .FirstOrDefault(p => p.Id == customPermissionId.Value);

                sections.Add(
                    $"CUSTOM_REQUEST | approved: {ResolveLabel(customPermission)}");
            }
            else if (customDecision == "rejected")
            {
                sections.Add(
                    $"CUSTOM_REQUEST | rejected: {customRequestDescription ?? "Custom request"}");
            }
        }

        return sections.Any()
            ? string.Join(" || ", sections)
            : "No reviewed items were attached to this request review.";
    }

    private static WorkerInfo? ResolveWorkerInfoForDisplay(
        Permission permission,
        Dictionary<Guid, WorkerInfo> workerInfoByFieldId)
    {
        if (permission.WorkerInfo != null)
            return permission.WorkerInfo;

        if (permission.FieldId.HasValue &&
            workerInfoByFieldId.TryGetValue(permission.FieldId.Value, out var presetInfo))
        {
            return presetInfo;
        }

        return null;
    }

    private static string ResolveLabel(
        Permission permission,
        Dictionary<Guid, WorkerInfo> workerInfoByFieldId)
    {
        var info = ResolveWorkerInfoForDisplay(permission, workerInfoByFieldId);

        return permission.Field?.Label
            ?? info?.Field?.Label
            ?? info?.CustomLabel
            ?? "Unknown";
    }

    private static string ResolveCategory(
        Permission permission,
        Dictionary<Guid, WorkerInfo> workerInfoByFieldId)
    {
        var info = ResolveWorkerInfoForDisplay(permission, workerInfoByFieldId);

        return permission.Field?.Category?.CategoryName
            ?? info?.Field?.Category?.CategoryName
            ?? "OtherInformation";
    }

    private static string ResolveType(
        Permission permission,
        Dictionary<Guid, WorkerInfo> workerInfoByFieldId)
    {
        var info = ResolveWorkerInfoForDisplay(permission, workerInfoByFieldId);

        return permission.Field?.AllowedType
            ?? info?.Type
            ?? "text";
    }

    private static string ResolveLabel(Permission? permission)
    {
        if (permission == null)
            return "Unknown";

        return permission.Field?.Label
            ?? permission.WorkerInfo?.Field?.Label
            ?? permission.WorkerInfo?.CustomLabel
            ?? "Unknown";
    }

    private static string ResolveCategory(Permission permission)
    {
        return permission.Field?.Category?.CategoryName
            ?? permission.WorkerInfo?.Field?.Category?.CategoryName
            ?? "OtherInformation";
    }

    private async Task<WorkerInfo?> ResolveWorkerInfoForApprovalAsync(
        Guid workerId,
        Permission permission,
        CancellationToken cancellationToken)
    {
        if (permission.InfoId.HasValue)
        {
            return await _context.WorkerInfos
                .FirstOrDefaultAsync(
                    w => w.Id == permission.InfoId.Value && w.WorkerId == workerId,
                    cancellationToken);
        }

        if (permission.FieldId.HasValue)
        {
            return await _context.WorkerInfos
                .FirstOrDefaultAsync(
                    w => w.WorkerId == workerId && w.FieldId == permission.FieldId.Value,
                    cancellationToken);
        }

        return null;
    }
}
