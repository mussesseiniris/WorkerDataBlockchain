using MediatR;
using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerActiveAccessServiceImpl : IEmployerActiveAccessService
{
    private readonly AppDbContext _context;
    private readonly ISupabaseStorageService _storage;
    private readonly IMediator _mediator;
    private readonly IBlockchainService _blockchainService;
    private readonly ILogger<EmployerActiveAccessServiceImpl> _logger;

    public EmployerActiveAccessServiceImpl(
        AppDbContext context,
        ISupabaseStorageService storage,
        IMediator mediator,
        IBlockchainService blockchainService,
        ILogger<EmployerActiveAccessServiceImpl> logger)
    {
        _context = context;
        _storage = storage;
        _mediator = mediator;
        _blockchainService = blockchainService;
        _logger = logger;
    }

    public async Task<List<EmployerActiveAccessDto>> GetActiveAccessAsync(
        Guid employerId,
        CancellationToken cancellationToken = default)
    {
        var employerExists = await _context.Employers
            .AsNoTracking()
            .AnyAsync(e => e.Id == employerId, cancellationToken);

        if (!employerExists)
        {
            throw new UnauthorizedAccessException("Current user is not an employer.");
        }

        var now = DateTime.UtcNow;

        var requests = await _context.Requests
            .AsNoTracking()
            .Include(r => r.Worker)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo!)
                    .ThenInclude(wi => wi.Field!)
                        .ThenInclude(f => f.Category)
            .Where(r => r.EmployerId == employerId && r.ExpiryDate != null && r.ExpiryDate > now)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = new List<EmployerActiveAccessDto>();

        foreach (var request in requests)
        {
            var approvedPerms = request.Permissions
                .Where(p => p.Status == PermissionStatus.Approved && p.WorkerInfo != null)
                .ToList();

            if (approvedPerms.Count == 0)
            {
                continue;
            }

            var grantedAt = approvedPerms.Max(p => p.LastUpdatedAt) ?? request.CreatedAt;
            var expiryDate = request.ExpiryDate ?? DateTime.UtcNow;

            var groups = approvedPerms
                .Select(p => new
                {
                    Permission = p,
                    CategoryName = ResolveCategory(p),
                    Label = ResolveLabel(p),
                    IsCustom = p.WorkerInfo!.CustomLabel != null
                })
                .GroupBy(x => x.CategoryName)
                .Select(g => new EmployerActiveAccessCategoryDto
                {
                    Name = g.Key,
                    Items = g.Select(x => new EmployerActiveAccessItemDto
                    {
                        PermissionId = x.Permission.Id,
                        Label = x.Label,
                        Type = x.Permission.WorkerInfo!.Type,
                        IsCustom = x.IsCustom
                    }).ToList()
                })
                .ToList();

            result.Add(new EmployerActiveAccessDto
            {
                RequestId = request.Id,
                WorkerId = request.WorkerId,
                WorkerName = request.Worker.Name,
                WorkerEmail = request.Worker.Email,
                Reason = request.Reason,
                GrantedAt = grantedAt,
                ExpiryDate = expiryDate,
                Categories = groups
            });
        }

        return result;
    }

    public async Task<EmployerRequestAccessViewDto> ViewRequestAsync(
        Guid employerId,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var dataRequest = await _context.Requests
            .Include(r => r.Worker)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.Field)
                    .ThenInclude(f => f!.Category)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo)
                    .ThenInclude(wi => wi!.Field)
                        .ThenInclude(f => f!.Category)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Request {requestId} not found");

        if (dataRequest.EmployerId != employerId)
        {
            throw new UnauthorizedAccessException("Request does not belong to the current employer.");
        }

        if (dataRequest.ExpiryDate == null || dataRequest.ExpiryDate <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Request has expired.");
        }

        var approvedPermissions = dataRequest.Permissions
            .Where(p => p.Status == PermissionStatus.Approved && p.WorkerInfo != null)
            .OrderBy(ResolveCategory)
            .ThenBy(ResolveLabel)
            .ToList();

        if (!approvedPermissions.Any())
        {
            throw new InvalidOperationException("No approved data is available for this request.");
        }

        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employerId, cancellationToken)
            ?? throw new KeyNotFoundException("Employer not found");

        var worker = dataRequest.Worker;

        var viewItems = new List<EmployerRequestAccessViewItemDto>();

        foreach (var permission in approvedPermissions)
        {
            var info = permission.WorkerInfo!;

            var item = new EmployerRequestAccessViewItemDto
            {
                PermissionId = permission.Id,
                Label = ResolveLabel(permission),
                Type = info.Type,
                IsCustom = info.CustomLabel != null
            };

            if (info.Type == "file")
            {
                if (string.IsNullOrWhiteSpace(info.Value))
                {
                    throw new InvalidOperationException(
                        $"No file path stored for item '{ResolveLabel(permission)}'.");
                }

                var signed = await _storage.CreateSignedUrlAsync(
                    info.Value,
                    900,
                    cancellationToken);

                item.Url = signed.Url;
                item.UrlExpiresAt = signed.ExpiresAt;
            }
            else
            {
                item.Value = info.Value;
            }

            viewItems.Add(item);
        }

        await _mediator.Send(
            new NotificationCommand(
                EmployerId: dataRequest.EmployerId,
                WorkerId: dataRequest.WorkerId,
                RequestId: dataRequest.Id,
                FieldLabel: null,
                Type: NotificationType.DataAccessed),
            cancellationToken);

        await LogRequestViewToBlockchainAsync(
            employer,
            worker,
            dataRequest,
            approvedPermissions,
            cancellationToken);

        var groupedCategories = viewItems
            .Select(item =>
            {
                var permission = approvedPermissions.First(p => p.Id == item.PermissionId);

                return new
                {
                    CategoryName = ResolveCategory(permission),
                    Item = item
                };
            })
            .GroupBy(x => x.CategoryName)
            .Select(g => new EmployerRequestAccessViewCategoryDto
            {
                Name = g.Key,
                Items = g.Select(x => x.Item).ToList()
            })
            .ToList();

        var requestGrantedAt = approvedPermissions.Max(p => p.LastUpdatedAt) ?? dataRequest.CreatedAt;

        return new EmployerRequestAccessViewDto
        {
            RequestId = dataRequest.Id,
            WorkerId = dataRequest.WorkerId,
            WorkerName = worker.Name,
            WorkerEmail = worker.Email,
            Reason = dataRequest.Reason,
            GrantedAt = requestGrantedAt,
            ExpiryDate = dataRequest.ExpiryDate.Value,
            ViewedAt = DateTime.UtcNow,
            Categories = groupedCategories
        };
    }

    private async Task LogRequestViewToBlockchainAsync(
        Employer employer,
        Worker worker,
        Request dataRequest,
        List<Permission> approvedPermissions,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(employer.PrivateKey))
        {
            throw new InvalidOperationException("EMPLOYER_PRIVATE_KEY_MISSING");
        }

        if (string.IsNullOrWhiteSpace(employer.BlockchainAddress))
        {
            throw new InvalidOperationException("EMPLOYER_BLOCKCHAIN_ADDRESS_MISSING");
        }

        if (string.IsNullOrWhiteSpace(worker.BlockchainAddress))
        {
            throw new InvalidOperationException("WORKER_BLOCKCHAIN_ADDRESS_MISSING");
        }

        var permissionIds = string.Join(
            ",",
            approvedPermissions
                .OrderBy(ResolveCategory)
                .ThenBy(ResolveLabel)
                .Select(p => p.Id.ToString()));

        var accessSummary = BuildRequestAccessSummary(approvedPermissions);

        _logger.LogWarning(
            "Writing request-level data access blockchain log. RequestId={RequestId}, PermissionCount={PermissionCount}, Summary={Summary}",
            dataRequest.Id,
            approvedPermissions.Count,
            accessSummary);

        var txHash = await _blockchainService.LogCategoryTransactionAsync(
            privateKey: employer.PrivateKey!,
            employerAddress: employer.BlockchainAddress!,
            workerAddress: worker.BlockchainAddress!,
            requestId: dataRequest.Id.ToString(),
            category: "RequestAccess",
            permissionIds: permissionIds,
            itemLabels: accessSummary,
            action: BlockchainAction.DataViewed,
            cancellationToken: cancellationToken);

        _logger.LogWarning(
            "Request-level data access blockchain log written successfully. RequestId={RequestId}, TxHash={TxHash}",
            dataRequest.Id,
            txHash);
    }

    private static string BuildRequestAccessSummary(List<Permission> permissions)
    {
        var groups = permissions
            .GroupBy(ResolveCategory)
            .OrderBy(g => g.Key)
            .ToList();

        if (!groups.Any())
        {
            return "No approved items were attached to this request access.";
        }

        var accessedText = string.Join(
            "; ",
            groups.Select(g =>
                $"{g.Key}: {string.Join(", ", g.OrderBy(ResolveLabel).Select(ResolveLabel))}"));

        return $"ACCESSED | {accessedText}";
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
}
