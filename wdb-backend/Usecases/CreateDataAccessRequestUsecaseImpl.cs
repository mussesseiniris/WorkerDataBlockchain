using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.Models;

namespace wdb_backend.Usecases;

/// <summary>
/// Creates a data access request and the related permission rows.
///
/// Blockchain design:
/// One employer submit request action writes one PermissionRequested blockchain record.
/// The record contains the whole request summary instead of one record per item.
/// </summary>
public class CreateDataAccessRequestUsecaseImpl : ICreateDataAccessRequestUsecase
{
    private readonly AppDbContext _context;
    private readonly IRequestService _requestService;
    private readonly IBlockchainService _blockchainService;
    private readonly ILogger<CreateDataAccessRequestUsecaseImpl> _logger;

    public CreateDataAccessRequestUsecaseImpl(
        AppDbContext context,
        IRequestService requestService,
        IBlockchainService blockchainService,
        ILogger<CreateDataAccessRequestUsecaseImpl> logger)
    {
        _context = context;
        _requestService = requestService;
        _blockchainService = blockchainService;
        _logger = logger;
    }

    public async Task CreateDataAccessRequest(
        List<Guid> selectedItemIds,
        Guid employerId,
        Guid workerId,
        string reason,
        string? customRequest = null,
        CancellationToken cancellationToken = default)
    {
        var hasSelectedItems = selectedItemIds != null && selectedItemIds.Count > 0;
        var hasCustomRequest = !string.IsNullOrWhiteSpace(customRequest);

        if (!hasSelectedItems && !hasCustomRequest)
            throw new InvalidOperationException("NO_SELECTED_ITEMS");

        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("REASON_REQUIRED");

        var distinctIds = selectedItemIds?
            .Distinct()
            .ToList() ?? new List<Guid>();

        var request = await _requestService.CreateAsync(
            employerId,
            workerId,
            reason.Trim(),
            customRequest,
            cancellationToken);

        foreach (var selectedId in distinctIds)
        {
            var workerInfo = await _context.WorkerInfos
                .Include(w => w.Field)
                    .ThenInclude(f => f!.Category)
                .FirstOrDefaultAsync(
                    w => w.Id == selectedId && w.WorkerId == workerId,
                    cancellationToken);

            if (workerInfo != null)
            {
                await EnsureNotAlreadyRequestedAsync(
                    employerId,
                    workerId,
                    workerInfo.FieldId,
                    workerInfo.Id,
                    cancellationToken);

                _context.Permissions.Add(new Permission
                {
                    RequestId = request.Id,
                    WorkerId = workerId,
                    FieldId = workerInfo.FieldId,
                    InfoId = workerInfo.Id,
                    Status = PermissionStatus.Pending,
                    LastUpdatedAt = DateTime.UtcNow
                });

                continue;
            }

            var field = await _context.Fields
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == selectedId, cancellationToken);

            if (field != null)
            {
                await EnsureNotAlreadyRequestedAsync(
                    employerId,
                    workerId,
                    field.Id,
                    null,
                    cancellationToken);

                _context.Permissions.Add(new Permission
                {
                    RequestId = request.Id,
                    WorkerId = workerId,
                    FieldId = field.Id,
                    InfoId = null,
                    Status = PermissionStatus.Pending,
                    LastUpdatedAt = DateTime.UtcNow
                });

                continue;
            }

            throw new KeyNotFoundException("SELECTED_ITEM_NOT_FOUND");
        }

        _context.Notifications.Add(new wdb_backend.Models.Notification
        {
            RecipientWorkerId = workerId,
            RecipientEmployerId = null,
            Type = "NEW_REQUEST",
            RequestId = request.Id,
            IsRead = false
        });

        await _context.SaveChangesAsync(cancellationToken);

        await LogRequestCreatedToBlockchainAsync(
            employerId,
            workerId,
            request.Id,
            customRequest,
            cancellationToken);
    }

    private async Task LogRequestCreatedToBlockchainAsync(
        Guid employerId,
        Guid workerId,
        Guid requestId,
        string? customRequest,
        CancellationToken cancellationToken)
    {
        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employerId, cancellationToken)
            ?? throw new KeyNotFoundException("EMPLOYER_NOT_FOUND_FOR_BLOCKCHAIN_LOG");

        var worker = await _context.Workers
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == workerId, cancellationToken)
            ?? throw new KeyNotFoundException("WORKER_NOT_FOUND_FOR_BLOCKCHAIN_LOG");

        if (string.IsNullOrWhiteSpace(employer.PrivateKey))
            throw new InvalidOperationException("EMPLOYER_PRIVATE_KEY_MISSING");

        if (string.IsNullOrWhiteSpace(employer.BlockchainAddress))
            throw new InvalidOperationException("EMPLOYER_BLOCKCHAIN_ADDRESS_MISSING");

        if (string.IsNullOrWhiteSpace(worker.BlockchainAddress))
            throw new InvalidOperationException("WORKER_BLOCKCHAIN_ADDRESS_MISSING");

        var permissionsToLog = await _context.Permissions
            .AsNoTracking()
            .Where(p => p.RequestId == requestId)
            .Include(p => p.Field)
                .ThenInclude(f => f!.Category)
            .Include(p => p.WorkerInfo)
                .ThenInclude(wi => wi!.Field)
                    .ThenInclude(f => f!.Category)
            .ToListAsync(cancellationToken);

        var permissionIds = string.Join(
            ",",
            permissionsToLog
                .OrderBy(ResolveCategory)
                .ThenBy(ResolveLabel)
                .Select(p => p.Id.ToString()));

        var requestSummary = BuildRequestCreatedSummary(
            permissionsToLog,
            customRequest);

        _logger.LogWarning(
            "Writing employer request blockchain log. RequestId={RequestId}, PermissionCount={PermissionCount}, Summary={Summary}",
            requestId,
            permissionsToLog.Count,
            requestSummary);

        var txHash = await _blockchainService.LogCategoryTransactionAsync(
            privateKey: employer.PrivateKey!,
            employerAddress: employer.BlockchainAddress!,
            workerAddress: worker.BlockchainAddress!,
            requestId: requestId.ToString(),
            category: "RequestAccess",
            permissionIds: permissionIds,
            itemLabels: requestSummary,
            action: BlockchainAction.PermissionRequested,
            cancellationToken: cancellationToken);

        _logger.LogWarning(
            "Employer request blockchain log written successfully. RequestId={RequestId}, TxHash={TxHash}",
            requestId,
            txHash);
    }

    private static string BuildRequestCreatedSummary(
        List<Permission> permissions,
        string? customRequest)
    {
        var sections = new List<string>();

        var requestedGroups = permissions
            .GroupBy(ResolveCategory)
            .OrderBy(g => g.Key)
            .ToList();

        if (requestedGroups.Any())
        {
            var requestedText = string.Join(
                "; ",
                requestedGroups.Select(g =>
                    $"{g.Key}: {string.Join(", ", g.OrderBy(ResolveLabel).Select(ResolveLabel))}"));

            sections.Add($"REQUESTED | {requestedText}");
        }

        if (!string.IsNullOrWhiteSpace(customRequest))
        {
            sections.Add($"CUSTOM_REQUEST | pending: {customRequest.Trim()}");
        }

        return sections.Any()
            ? string.Join(" || ", sections)
            : "No requested items were attached to this access request.";
    }

    private static string ResolveLabel(Permission permission)
    {
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

    private async Task EnsureNotAlreadyRequestedAsync(
        Guid employerId,
        Guid workerId,
        Guid? fieldId,
        Guid? infoId,
        CancellationToken cancellationToken)
    {
        var alreadyRequested = await _context.Permissions
            .Include(p => p.Request)
            .AnyAsync(
                p =>
                    p.WorkerId == workerId &&
                    p.Request.EmployerId == employerId &&
                    (p.Status == PermissionStatus.Pending ||
                     p.Status == PermissionStatus.Approved) &&
                    (
                        (fieldId.HasValue && p.FieldId == fieldId.Value) ||
                        (infoId.HasValue && p.InfoId == infoId.Value)
                    ),
                cancellationToken);

        if (alreadyRequested)
            throw new InvalidOperationException("ITEM_ALREADY_REQUESTED");
    }
}
