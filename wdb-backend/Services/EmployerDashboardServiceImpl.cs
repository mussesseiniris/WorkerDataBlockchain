using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class EmployerDashboardServiceImpl : IEmployerDashboardService
{
    private const int DashboardPreviewLimit = 3;

    private readonly AppDbContext _context;

    public EmployerDashboardServiceImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmployerDashboardDto> GetDashboardAsync(
        Guid employerId,
        CancellationToken cancellationToken = default)
    {
        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employerId, cancellationToken);

        if (employer == null)
        {
            throw new UnauthorizedAccessException("Current user is not an employer.");
        }

        var requests = await _context.Requests
            .AsNoTracking()
            .Include(r => r.Worker)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.Field!)
                    .ThenInclude(f => f.Category)
            .Include(r => r.Permissions)
                .ThenInclude(p => p.WorkerInfo!)
                    .ThenInclude(wi => wi.Field!)
                        .ThenInclude(f => f.Category)
            .Where(r => r.EmployerId == employerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var perRequestStatus = requests.ToDictionary(r => r.Id, GetRequestStatus);

        var pendingRequests = perRequestStatus.Values.Count(status => status == "Pending");
        var totalRequests = requests.Count;

        var summary = new EmployerDashboardSummaryDto
        {
            PendingRequests = pendingRequests,
            ReviewedRequests = totalRequests - pendingRequests,
            TotalRequests = totalRequests
        };

        var recentRequests = requests
            .Take(DashboardPreviewLimit)
            .Select(r => new EmployerRecentRequestDto
            {
                RequestId = r.Id,
                WorkerName = r.Worker.Name,
                RequestedFields = r.Permissions
                    .Select(GetPermissionLabel)
                    .Distinct()
                    .ToList(),
                Reason = r.Reason,
                Status = perRequestStatus[r.Id],
                LastUpdatedAt = r.Permissions.Any()
                    ? (r.Permissions.Max(p => p.LastUpdatedAt) ?? r.CreatedAt)
                    : r.CreatedAt
            })
            .ToList();

        return new EmployerDashboardDto
        {
            Company = new EmployerCompanyInfoDto
            {
                Name = employer.Name,
                Email = employer.Email,
                Verified = employer.Verified
            },
            Summary = summary,
            RecentRequests = recentRequests
        };
    }

    private static string GetPermissionLabel(Permission p)
    {
        if (p.Field != null) return p.Field.Label;

        if (p.WorkerInfo != null)
        {
            return p.WorkerInfo.CustomLabel
                   ?? p.WorkerInfo.Field?.Label
                   ?? "Unknown";
        }

        return "Unknown";
    }

    private static string GetRequestStatus(Request request)
    {
        var perms = request.Permissions;

        if (perms.Count == 0)
        {
            return "Pending";
        }

        if (perms.Any(p => p.Status == PermissionStatus.Revoked))
        {
            return "Revoked";
        }

        if (perms.All(p => p.Status == PermissionStatus.Pending))
        {
            return "Pending";
        }

        if (perms.All(p => p.Status == PermissionStatus.Rejected))
        {
            return "Rejected";
        }

        if (perms.All(p => p.Status == PermissionStatus.Approved)
            && request.CustomRequestStatus != "rejected")
        {
            return "Approved";
        }

        return "PartiallyApproved";
    }
}
