using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;

namespace wdb_backend.Services;

public class EmployerDashboardServiceImpl : IEmployerDashboardService
{
    private readonly AppDbContext _context;

    public EmployerDashboardServiceImpl(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmployerDashboardDto> GetDashboardAsync(
        Guid employerId,
        CancellationToken cancellationToken = default
    )
    {
        var employer = await _context.Employers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employerId, cancellationToken);

        if (employer == null)
        {
            throw new UnauthorizedAccessException("Current user is not an employer.");
        }

        var now = DateTime.UtcNow;

        var recentRequests = await GetRecentRequestsAsync(
            employerId,
            now,
            cancellationToken
        );

        var pendingRequests = recentRequests.Count(request =>
            request.Status == "Pending"
        );

        var availableRequests = recentRequests.Count(request =>
            request.Status == "Available"
        );

        var partialRequests = recentRequests.Count(request =>
            request.Status == "Partial"
        );

        return new EmployerDashboardDto
        {
            Company = new EmployerCompanyInfoDto
            {
                Name = employer.Name,
                Email = employer.Email,
                Verified = employer.Verified
            },
            Summary = new EmployerDashboardSummaryDto
            {
                PendingRequests = pendingRequests,
                AvailableRequests = availableRequests,
                PartialRequests = partialRequests
            },
            RecentRequests = recentRequests
        };
    }

    private async Task<List<EmployerRecentRequestDto>> GetRecentRequestsAsync(
        Guid employerId,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        var recentRequestRows = await (
            from request in _context.Requests.AsNoTracking()
            join worker in _context.Workers.AsNoTracking()
                on request.WorkerId equals worker.Id
            join permission in _context.Permissions.AsNoTracking()
                on request.Id equals permission.RequestId
            join workerInfo in _context.WorkerInfos.AsNoTracking()
                on permission.InfoId equals workerInfo.Id
            where request.EmployerId == employerId
            orderby permission.LastUpdatedAt descending
            select new RecentRequestRow
            {
                RequestId = request.Id,
                WorkerName = worker.Name,
                InfoDesc = workerInfo.Desc,
                Reason = request.Reason,
                Status = permission.Status,
                ExpiryDate = permission.ExpiryDate,
                LastUpdatedAt = permission.LastUpdatedAt
            }
        ).ToListAsync(cancellationToken);

        return recentRequestRows
            .GroupBy(row => row.RequestId)
            .Select(group =>
            {
                var rows = group.ToList();

                return new EmployerRecentRequestDto
                {
                    RequestId = group.Key,
                    WorkerName = rows.First().WorkerName,
                    RequestedFields = rows
                        .Select(row => row.InfoDesc)
                        .Distinct()
                        .ToList(),
                    Reason = rows.First().Reason,
                    Status = GetRequestDisplayStatus(rows, now),
                    LastUpdatedAt = rows.Max(row => row.LastUpdatedAt)
                };
            })
            .OrderByDescending(request => request.LastUpdatedAt)
            .Take(8)
            .ToList();
    }

    private static string GetRequestDisplayStatus(
        List<RecentRequestRow> rows,
        DateTime now
    )
    {
        if (rows.Count == 0)
        {
            return "Unavailable";
        }

        var allPending = rows.All(row =>
            row.Status == PermissionStatus.Pending
        );

        if (allPending)
        {
            return "Pending";
        }

        var allAvailable = rows.All(row =>
            row.Status == PermissionStatus.Approved &&
            row.ExpiryDate.HasValue &&
            row.ExpiryDate.Value > now
        );

        if (allAvailable)
        {
            return "Available";
        }

        var allRejected = rows.All(row =>
            row.Status == PermissionStatus.Rejected
        );

        if (allRejected)
        {
            return "Unavailable";
        }

        var allApprovedButExpired = rows.All(row =>
            row.Status == PermissionStatus.Approved &&
            row.ExpiryDate.HasValue &&
            row.ExpiryDate.Value <= now
        );

        if (allApprovedButExpired)
        {
            return "Unavailable";
        }

        return "Partial";
    }

    private class RecentRequestRow
    {
        public Guid RequestId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public string InfoDesc { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public PermissionStatus Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
