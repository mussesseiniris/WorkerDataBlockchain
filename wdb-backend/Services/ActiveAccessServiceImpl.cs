using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class ActiveAccessServiceImpl : IActiveAccessService
{
    private readonly AppDbContext _context;
    private readonly IBlockchainService _blockchainService;
    private readonly ILogger<ActiveAccessServiceImpl> _logger;

    public ActiveAccessServiceImpl(
        AppDbContext context,
        IBlockchainService blockchainService,
        ILogger<ActiveAccessServiceImpl> logger)
    {
        _context = context;
        _blockchainService = blockchainService;
        _logger = logger;
    }

    /// <summary>
    /// Return active approved permissions for the worker.
    /// A permission is active only when:
    /// - permission.status = Approved
    /// - related request has an expiry date
    /// - related request has not expired
    /// </summary>
    public async Task<List<ActiveAccessDto>> GetActiveAccessAsync(
        Guid workerId,
        string? company = null,
        string? dataType = null)
    {
        var now = DateTime.UtcNow;

        var requests = await _context.Requests
            .Where(r =>
                r.WorkerId == workerId &&
                r.ExpiryDate.HasValue &&
                r.ExpiryDate.Value > now &&
                r.Permissions.Any(p => p.Status == PermissionStatus.Approved))
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo)
                    .ThenInclude(w => w!.Field)
                        .ThenInclude(f => f!.Category)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.Field)
                    .ThenInclude(f => f!.Category)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var rows = new List<ActiveAccessDto>();

        foreach (var request in requests)
        {
            var employer = await _context.Employers
                .FirstOrDefaultAsync(e => e.Id == request.EmployerId);

            var companyName = employer?.Name ?? "Unknown";

            if (!string.IsNullOrWhiteSpace(company) &&
                !companyName.Contains(company, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var approvedPermissions = request.Permissions
                .Where(p => p.Status == PermissionStatus.Approved)
                .ToList();

            var infoItems = approvedPermissions
                .Select(p =>
                {
                    var category = ResolveCategory(p);

                    return new ActiveAccessInfoDto
                    {
                        PermissionId = p.Id,
                        DataType = ResolveLabel(p),
                        Category = category,
                        CategoryLabel = ToCategoryLabel(category)
                    };
                })
                .OrderBy(i => i.CategoryLabel)
                .ThenBy(i => i.DataType)
                .ToList();

            if (!string.IsNullOrWhiteSpace(dataType))
            {
                infoItems = infoItems
                    .Where(i =>
                        i.DataType.Equals(dataType, StringComparison.OrdinalIgnoreCase) ||
                        i.Category.Equals(dataType, StringComparison.OrdinalIgnoreCase) ||
                        i.CategoryLabel.Equals(dataType, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!infoItems.Any())
                    continue;
            }

            rows.Add(new ActiveAccessDto
            {
                RequestId = request.Id,
                CompanyName = companyName,
                GrantedAt = approvedPermissions.Max(p => p.LastUpdatedAt) ?? DateTime.UtcNow,
                Reason = request.Reason,
                WorkerInfo = infoItems
            });
        }

        return rows
            .OrderByDescending(r => r.GrantedAt)
            .ToList();
    }

    /// <summary>
    /// Revoke all approved permissions under one active request/access grant.
    /// This updates database status, notifies the employer, and writes one
    /// request-level PermissionRevoked record to blockchain.
    /// </summary>
    public async Task RevokeRequestAccessAsync(
        Guid workerId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
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

        if (!dataRequest.ExpiryDate.HasValue ||
            dataRequest.ExpiryDate.Value <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("REQUEST_EXPIRED");
        }

        var approvedPermissions = dataRequest.Permissions
            .Where(p => p.Status == PermissionStatus.Approved)
            .ToList();

        if (!approvedPermissions.Any())
            throw new InvalidOperationException("NO_APPROVED_PERMISSIONS");

        foreach (var permission in approvedPermissions)
        {
            permission.Status = PermissionStatus.Revoked;
            permission.LastUpdatedAt = DateTime.UtcNow;
        }

        _context.Notifications.Add(new wdb_backend.Models.Notification
        {
            RecipientWorkerId = null,
            RecipientEmployerId = dataRequest.EmployerId,
            Type = "ACCESS_REVOKED",
            RequestId = dataRequest.Id,
            IsRead = false
        });

        await _context.SaveChangesAsync(cancellationToken);

        await LogRequestRevokeToBlockchainAsync(
            workerId,
            dataRequest.Id,
            approvedPermissions.Select(p => p.Id).ToList(),
            cancellationToken);
    }

    private async Task LogRequestRevokeToBlockchainAsync(
        Guid workerId,
        Guid requestId,
        List<Guid> revokedPermissionIds,
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

            var permissionsToLog = await _context.Permissions
                .AsNoTracking()
                .Where(p =>
                    p.RequestId == requestId &&
                    revokedPermissionIds.Contains(p.Id))
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

            var revokeSummary = BuildRequestRevokeSummary(permissionsToLog);

            _logger.LogWarning(
                "Writing request-level revoke blockchain log. RequestId={RequestId}, PermissionCount={PermissionCount}, Summary={Summary}",
                requestId,
                permissionsToLog.Count,
                revokeSummary);

            var txHash = await _blockchainService.LogCategoryTransactionAsync(
                privateKey: worker.PrivateKey!,
                employerAddress: employer.BlockchainAddress!,
                workerAddress: worker.BlockchainAddress!,
                requestId: dataRequest.Id.ToString(),
                category: "RequestAccess",
                permissionIds: permissionIds,
                itemLabels: revokeSummary,
                action: BlockchainAction.PermissionRevoked,
                cancellationToken: cancellationToken);

            _logger.LogWarning(
                "Request-level revoke blockchain log written successfully. RequestId={RequestId}, TxHash={TxHash}",
                requestId,
                txHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "REQUEST REVOKE BLOCKCHAIN LOG FAILED. RequestId={RequestId}, Error={ErrorMessage}",
                requestId,
                ex.Message);

            // Keep this throw during testing so blockchain failures are visible.
            // After demo stability is confirmed, this can be removed if the team
            // wants database revoke to succeed even when blockchain is unavailable.
            throw;
        }
    }

    private static string BuildRequestRevokeSummary(List<Permission> permissions)
    {
        var revokedGroups = permissions
            .GroupBy(ResolveCategory)
            .OrderBy(g => g.Key)
            .ToList();

        if (!revokedGroups.Any())
            return "No revoked items were attached to this access revoke.";

        var revokedText = string.Join(
            "; ",
            revokedGroups.Select(g =>
                $"{g.Key}: {string.Join(", ", g.OrderBy(ResolveLabel).Select(ResolveLabel))}"));

        return $"REVOKED | {revokedText}";
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

    private static string ToCategoryLabel(string category)
    {
        return category switch
        {
            "PersonalInformation" => "Personal Information",
            "MedicalInformation" => "Medical Information",
            "CareerInformation" => "Career Information",
            "FinancialInformation" => "Financial Information",
            "WorkplaceInformation" => "Workplace Information",
            "OtherInformation" => "Other Information",
            _ => category
        };
    }
}
