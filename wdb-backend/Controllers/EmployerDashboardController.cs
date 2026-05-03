using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wdb_backend.Common;
using wdb_backend.Data;
using wdb_backend.DTOs;

namespace wdb_backend.Controllers;

[ApiController]
[Route("api/employers")]
public class EmployerDashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployerDashboardController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("me/dashboard")]
    public async Task<IActionResult> GetMyDashboard(CancellationToken cancellationToken)
    {
        var employerId = GetCurrentUserId();

        if (employerId == null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Missing or invalid user id in token."
            });
        }

        var employerExists = await _context.Employers
            .AsNoTracking()
            .AnyAsync(e => e.Id == employerId.Value, cancellationToken);

        if (!employerExists)
        {
            return Forbid();
        }

        var now = DateTime.UtcNow;
        var expiringSoonLimit = now.AddDays(30);

        var pendingRequests = await _context.Permissions
            .AsNoTracking()
            .Join(
                _context.Requests,
                permission => permission.RequestId,
                request => request.Id,
                (permission, request) => new { permission, request }
            )
            .CountAsync(
                x => x.request.EmployerId == employerId.Value
                    && x.permission.Status == PermissionStatus.Pending,
                cancellationToken
            );

        var activeApprovedAccess = await _context.Permissions
            .AsNoTracking()
            .Join(
                _context.Requests,
                permission => permission.RequestId,
                request => request.Id,
                (permission, request) => new { permission, request }
            )
            .CountAsync(
                x => x.request.EmployerId == employerId.Value
                    && x.permission.Status == PermissionStatus.Approved
                    && x.permission.ExpiryDate > now,
                cancellationToken
            );

        var expiringSoon = await _context.Permissions
            .AsNoTracking()
            .Join(
                _context.Requests,
                permission => permission.RequestId,
                request => request.Id,
                (permission, request) => new { permission, request }
            )
            .CountAsync(
                x => x.request.EmployerId == employerId.Value
                    && x.permission.Status == PermissionStatus.Approved
                    && x.permission.ExpiryDate > now
                    && x.permission.ExpiryDate <= expiringSoonLimit,
                cancellationToken
            );

        var recentRequestRows = await (
            from request in _context.Requests.AsNoTracking()
            join worker in _context.Workers.AsNoTracking()
                on request.WorkerId equals worker.Id
            join permission in _context.Permissions.AsNoTracking()
                on request.Id equals permission.RequestId
            join workerInfo in _context.WorkerInfos.AsNoTracking()
                on permission.InfoId equals workerInfo.Id
            where request.EmployerId == employerId.Value
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
        )
        .ToListAsync(cancellationToken);

        var recentRequests = recentRequestRows
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

        var dashboard = new EmployerDashboardDto
        {
            Summary = new EmployerDashboardSummaryDto
            {
                PendingRequests = pendingRequests,
                ActiveApprovedAccess = activeApprovedAccess,
                ExpiringSoon = expiringSoon
            },
            RecentRequests = recentRequests
        };

        return Ok(new
        {
            success = true,
            data = dashboard,
            message = "Employer dashboard data retrieved."
        });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }

    private static string GetRequestDisplayStatus(
        List<RecentRequestRow> rows,
        DateTime now
    )
    {
        if (rows.All(row => row.Status == PermissionStatus.Pending))
        {
            return "Pending";
        }

        if (rows.All(row => row.Status == PermissionStatus.Rejected))
        {
            return "Rejected";
        }

        if (rows.All(row =>
                row.Status == PermissionStatus.Approved
                && row.ExpiryDate > now))
        {
            return "Approved";
        }

        if (rows.All(row =>
                row.Status == PermissionStatus.Approved
                && row.ExpiryDate <= now))
        {
            return "Expired";
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
        public DateTime ExpiryDate { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
