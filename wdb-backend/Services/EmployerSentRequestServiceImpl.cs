using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerSentRequestServiceImpl : IEmployerSentRequestService
{
    private readonly AppDbContext _context;

    public EmployerSentRequestServiceImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployerSentRequestDto>> GetSentRequestsAsync(
        Guid employerId,
        CancellationToken cancellationToken = default)
    {
        var employerExists = await _context.Employers
            .AsNoTracking()
            .AnyAsync(e => e.Id == employerId, cancellationToken);

        if (!employerExists)
            throw new UnauthorizedAccessException("Current user is not an employer.");

        var requests = await _context.Requests
            .AsNoTracking()
            .Include(r => r.Worker)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.Field)
                    .ThenInclude(f => f!.Category)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo)
                    .ThenInclude(wi => wi!.Field)
                        .ThenInclude(f => f!.Category)
            .Where(r => r.EmployerId == employerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(request =>
        {
            var permissionItems = request.Permissions
                .OrderBy(p => ResolveCategoryName(p))
                .ThenBy(p => ResolveLabel(p))
                .Select(p => new EmployerSentRequestItemDto
                {
                    PermissionId = p.Id,
                    CategoryName = ResolveCategoryName(p),
                    Label = ResolveLabel(p),
                    Status = p.Status,
                    IsCustom = p.WorkerInfo?.CustomLabel != null
                })
                .ToList();

            return new EmployerSentRequestDto
            {
                RequestId = request.Id,
                WorkerId = request.WorkerId,
                WorkerName = request.Worker.Name,
                WorkerEmail = request.Worker.Email,
                Reason = request.Reason,

                // Pending requests do not have an expiry date yet.
                // DateTime.MinValue keeps the current DTO contract without pretending
                // the employer has set an expiry date.
                ExpiryDate = request.ExpiryDate ?? DateTime.MinValue,

                CreatedAt = request.CreatedAt,
                LastUpdatedAt = request.Permissions
                    .Select(p => p.LastUpdatedAt)
                    .Where(d => d.HasValue)
                    .Select(d => d!.Value)
                    .DefaultIfEmpty(request.CreatedAt)
                    .Max(),

                CustomRequest = request.CustomRequest,
                CustomRequestStatus = request.CustomRequestStatus,
                Items = permissionItems
            };
        }).ToList();
    }

    private static string ResolveCategoryName(Permission permission)
    {
        return permission.Field?.Category?.CategoryName
            ?? permission.WorkerInfo?.Field?.Category?.CategoryName
            ?? "OtherInformation";
    }

    private static string ResolveLabel(Permission permission)
    {
        return permission.Field?.Label
            ?? permission.WorkerInfo?.Field?.Label
            ?? permission.WorkerInfo?.CustomLabel
            ?? "Unknown";
    }
}
