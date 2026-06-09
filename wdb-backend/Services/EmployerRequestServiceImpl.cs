using MediatR;
using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerRequestServiceImpl : IEmployerRequestService
{
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;
    private readonly IBlockchainService _blockchainService;
    private readonly ILogger<EmployerRequestServiceImpl> _logger;

    public EmployerRequestServiceImpl(
        AppDbContext context,
        IMediator mediator,
        IBlockchainService blockchainService,
        ILogger<EmployerRequestServiceImpl> logger)
    {
        _context = context;
        _mediator = mediator;
        _blockchainService = blockchainService;
        _logger = logger;
    }

    public async Task<EmployerRequestCatalogDto?> GetCatalogAsync(
        string workerEmail,
        CancellationToken cancellationToken = default)
    {
        var worker = await _context.Workers
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Email == workerEmail, cancellationToken);

        if (worker == null)
        {
            return null;
        }

        var categories = await _context.Categories
            .AsNoTracking()
            .Include(c => c.Fields)
            .ToListAsync(cancellationToken);

        // Worker's custom items live under the OtherInformation category by convention;
        // they are identified by having a non-null custom_label.
        var customItems = await _context.WorkerInfos
            .AsNoTracking()
            .Where(wi => wi.WorkerId == worker.Id && wi.CustomLabel != null)
            .ToListAsync(cancellationToken);

        var otherCategory = categories.FirstOrDefault(c => c.CategoryName == "OtherInformation");

        return new EmployerRequestCatalogDto
        {
            Worker = new EmployerRequestCatalogWorkerDto
            {
                Id = worker.Id,
                Name = worker.Name,
                Email = worker.Email
            },
            Categories = categories.Select(c => new EmployerRequestCatalogCategoryDto
            {
                Id = c.Id,
                Name = c.CategoryName,
                PresetFields = c.Fields
                    .OrderBy(f => f.Label)
                    .Select(f => new EmployerRequestCatalogPresetFieldDto
                    {
                        FieldId = f.Id,
                        Label = f.Label,
                        AllowedType = f.AllowedType
                    })
                    .ToList(),
                CustomItems = (otherCategory != null && c.Id == otherCategory.Id)
                    ? customItems.Select(wi => new EmployerRequestCatalogCustomItemDto
                    {
                        WorkerInfoId = wi.Id,
                        Label = wi.CustomLabel ?? string.Empty,
                        Type = wi.Type
                    }).ToList()
                    : new List<EmployerRequestCatalogCustomItemDto>()
            }).ToList()
        };
    }

    public async Task<CreateEmployerRequestResultDto> CreateAsync(
        Guid employerId,
        CreateEmployerRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var worker = await _context.Workers
            .FirstOrDefaultAsync(w => w.Email == dto.WorkerEmail, cancellationToken)
            ?? throw new KeyNotFoundException($"Worker {dto.WorkerEmail} not found");

        var hasAnyItem = dto.PresetFieldIds.Count > 0
                         || dto.CustomWorkerInfoIds.Count > 0
                         || !string.IsNullOrWhiteSpace(dto.CustomRequest);

        if (!hasAnyItem)
        {
            throw new ArgumentException(
                "Request must include at least one field, custom item, or custom request");
        }

        var hasCustomRequest = !string.IsNullOrWhiteSpace(dto.CustomRequest);

        var request = new Request
        {
            EmployerId = employerId,
            WorkerId = worker.Id,
            Reason = dto.Reason,
            // Expiry is set later by the worker during approval.
            ExpiryDate = null,
            CustomRequest = hasCustomRequest ? dto.CustomRequest : null,
            CustomRequestStatus = hasCustomRequest ? "pending" : null
        };

        _context.Requests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);

        // Preset fields: permission references the field definition; info_id stays null
        // until the worker approves and an actual worker_info row is associated.
        foreach (var fieldId in dto.PresetFieldIds.Distinct())
        {
            _context.Permissions.Add(new Permission
            {
                RequestId = request.Id,
                WorkerId = worker.Id,
                FieldId = fieldId,
                InfoId = null,
                Status = PermissionStatus.Pending,
                LastUpdatedAt = DateTime.UtcNow
            });
        }

        // Existing custom items: permission points directly at the worker_info row.
        foreach (var workerInfoId in dto.CustomWorkerInfoIds.Distinct())
        {
            _context.Permissions.Add(new Permission
            {
                RequestId = request.Id,
                WorkerId = worker.Id,
                FieldId = null,
                InfoId = workerInfoId,
                Status = PermissionStatus.Pending,
                LastUpdatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _mediator.Send(
            new NotificationCommand(
                EmployerId: employerId,
                WorkerId: worker.Id,
                RequestId: request.Id,
                FieldLabel: null,
                Type: NotificationType.NewRequest),
            cancellationToken);

        await LogRequestCreatedToBlockchainAsync(
            employerId,
            worker.Id,
            request.Id,
            cancellationToken);

        return new CreateEmployerRequestResultDto { RequestId = request.Id };
    }

    private async Task LogRequestCreatedToBlockchainAsync(
        Guid employerId,
        Guid workerId,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        try
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

            var dataRequest = await _context.Requests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken)
                ?? throw new KeyNotFoundException("REQUEST_NOT_FOUND_FOR_BLOCKCHAIN_LOG");

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
                dataRequest.CustomRequest);

            _logger.LogWarning(
                "Writing request-level creation blockchain log. RequestId={RequestId}, PermissionCount={PermissionCount}, Summary={Summary}",
                requestId,
                permissionsToLog.Count,
                requestSummary);

            var txHash = await _blockchainService.LogCategoryTransactionAsync(
                privateKey: employer.PrivateKey!,
                employerAddress: employer.BlockchainAddress!,
                workerAddress: worker.BlockchainAddress!,
                requestId: requestId.ToString(),
                category: "RequestCreated",
                permissionIds: permissionIds,
                itemLabels: requestSummary,
                action: BlockchainAction.PermissionRequested,
                cancellationToken: cancellationToken);

            _logger.LogWarning(
                "Request-level creation blockchain log written successfully. RequestId={RequestId}, TxHash={TxHash}",
                requestId,
                txHash);
        }
        catch (Exception ex)
        {
            // Keep request creation working even if the local blockchain is unavailable.
            // The database request and worker notification should still succeed.
            _logger.LogWarning(
                ex,
                "REQUEST CREATED BLOCKCHAIN LOG FAILED. RequestId={RequestId}, Error={ErrorMessage}",
                requestId,
                ex.Message);
        }
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
            : "No requested items were attached to this request.";
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
