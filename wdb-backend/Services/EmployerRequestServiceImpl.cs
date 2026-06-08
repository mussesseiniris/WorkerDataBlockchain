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

        // Worker's custom items live under the OtherInformation category by convention.
        var customItems = await _context.WorkerInfos
            .AsNoTracking()
            .Where(wi => wi.WorkerId == worker.Id && wi.CustomLabel != null)
            .ToListAsync(cancellationToken);

        var otherCategory = categories
            .FirstOrDefault(c => c.CategoryName == "OtherInformation");

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
                CustomItems = otherCategory != null && c.Id == otherCategory.Id
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
        if (string.IsNullOrWhiteSpace(dto.WorkerEmail))
        {
            throw new ArgumentException("Worker email is required");
        }

        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            throw new ArgumentException("Reason is required");
        }

        var employer = await _context.Employers
            .FirstOrDefaultAsync(e => e.Id == employerId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Current user is not an employer.");

        var worker = await _context.Workers
            .FirstOrDefaultAsync(w => w.Email == dto.WorkerEmail, cancellationToken)
            ?? throw new KeyNotFoundException($"Worker {dto.WorkerEmail} not found");

        var hasAnyItem =
            dto.PresetFieldIds.Count > 0 ||
            dto.CustomWorkerInfoIds.Count > 0 ||
            !string.IsNullOrWhiteSpace(dto.CustomRequest);

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
            Reason = dto.Reason.Trim(),

            // Employer does not set expiry date.
            // Worker sets this during approval.
            ExpiryDate = null,

            CustomRequest = hasCustomRequest ? dto.CustomRequest!.Trim() : null,
            CustomRequestStatus = hasCustomRequest ? "pending" : null
        };

        _context.Requests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);

        var createdPermissions = new List<Permission>();

        foreach (var fieldId in dto.PresetFieldIds.Distinct())
        {
            var permission = new Permission
            {
                RequestId = request.Id,
                WorkerId = worker.Id,
                FieldId = fieldId,
                InfoId = null,
                Status = PermissionStatus.Pending,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.Permissions.Add(permission);
            createdPermissions.Add(permission);
        }

        foreach (var workerInfoId in dto.CustomWorkerInfoIds.Distinct())
        {
            var permission = new Permission
            {
                RequestId = request.Id,
                WorkerId = worker.Id,
                FieldId = null,
                InfoId = workerInfoId,
                Status = PermissionStatus.Pending,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.Permissions.Add(permission);
            createdPermissions.Add(permission);
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
            employer,
            worker,
            request,
            createdPermissions,
            cancellationToken);

        return new CreateEmployerRequestResultDto { RequestId = request.Id };
    }

    private async Task LogRequestCreatedToBlockchainAsync(
        Employer employer,
        Worker worker,
        Request request,
        List<Permission> createdPermissions,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(employer.PrivateKey))
                throw new InvalidOperationException("EMPLOYER_PRIVATE_KEY_MISSING");

            if (string.IsNullOrWhiteSpace(employer.BlockchainAddress))
                throw new InvalidOperationException("EMPLOYER_BLOCKCHAIN_ADDRESS_MISSING");

            if (string.IsNullOrWhiteSpace(worker.BlockchainAddress))
                throw new InvalidOperationException("WORKER_BLOCKCHAIN_ADDRESS_MISSING");

            var permissionIds = string.Join(
                ",",
                createdPermissions
                    .OrderBy(p => p.Id)
                    .Select(p => p.Id.ToString()));

            var requestSummary = await BuildRequestCreatedSummaryAsync(
                request,
                createdPermissions,
                cancellationToken);

            _logger.LogWarning(
                "Writing request-created blockchain log. RequestId={RequestId}, PermissionCount={PermissionCount}, Summary={Summary}",
                request.Id,
                createdPermissions.Count,
                requestSummary);

            var txHash = await _blockchainService.LogCategoryTransactionAsync(
                privateKey: employer.PrivateKey!,
                employerAddress: employer.BlockchainAddress!,
                workerAddress: worker.BlockchainAddress!,
                requestId: request.Id.ToString(),
                category: "RequestCreated",
                permissionIds: permissionIds,
                itemLabels: requestSummary,
                action: BlockchainAction.PermissionRequested,
                cancellationToken: cancellationToken);

            _logger.LogWarning(
                "Request-created blockchain log written successfully. RequestId={RequestId}, TxHash={TxHash}",
                request.Id,
                txHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "REQUEST CREATED BLOCKCHAIN LOG FAILED. RequestId={RequestId}, Error={ErrorMessage}",
                request.Id,
                ex.Message);

            // During testing/demo, throw so missing blockchain records are visible.
            // If the team later wants request creation to succeed even when blockchain
            // is unavailable, change this to return instead of throw.
            throw;
        }
    }

    private async Task<string> BuildRequestCreatedSummaryAsync(
        Request request,
        List<Permission> permissions,
        CancellationToken cancellationToken)
    {
        var fieldIds = permissions
            .Where(p => p.FieldId.HasValue)
            .Select(p => p.FieldId!.Value)
            .Distinct()
            .ToList();

        var workerInfoIds = permissions
            .Where(p => p.InfoId.HasValue)
            .Select(p => p.InfoId!.Value)
            .Distinct()
            .ToList();

        var fieldRows = await _context.Fields
            .AsNoTracking()
            .Where(f => fieldIds.Contains(f.Id))
            .Include(f => f.Category)
            .ToListAsync(cancellationToken);

        var workerInfoRows = await _context.WorkerInfos
            .AsNoTracking()
            .Where(wi => workerInfoIds.Contains(wi.Id))
            .Include(wi => wi.Field)
                .ThenInclude(f => f!.Category)
            .ToListAsync(cancellationToken);

        var sections = new List<string>();

        var presetGroups = fieldRows
            .GroupBy(f => f.Category?.CategoryName ?? "OtherInformation")
            .OrderBy(g => g.Key)
            .ToList();

        if (presetGroups.Any())
        {
            var presetText = string.Join(
                "; ",
                presetGroups.Select(g =>
                    $"{g.Key}: {string.Join(", ", g.OrderBy(f => f.Label).Select(f => f.Label))}"));

            sections.Add($"REQUESTED_PRESET | {presetText}");
        }

        var customItemLabels = workerInfoRows
            .Select(wi => wi.CustomLabel ?? wi.Field?.Label ?? "Unknown")
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .OrderBy(label => label)
            .ToList();

        if (customItemLabels.Any())
        {
            sections.Add($"REQUESTED_EXISTING_CUSTOM | {string.Join(", ", customItemLabels)}");
        }

        if (!string.IsNullOrWhiteSpace(request.CustomRequest))
        {
            sections.Add($"REQUESTED_NEW_CUSTOM | {request.CustomRequest}");
        }

        return sections.Any()
            ? string.Join(" || ", sections)
            : "REQUEST_CREATED | No requested items were attached.";
    }
}
