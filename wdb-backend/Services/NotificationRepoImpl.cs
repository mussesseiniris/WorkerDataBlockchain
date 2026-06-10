using Microsoft.EntityFrameworkCore;
using wdb_backend.Abstractions;
using wdb_backend.Data;
using wdb_backend.DTOs;
using wdb_backend.Models;

namespace wdb_backend.Services;

public class NotificationRepoImpl : INotificationRepository
{
    private readonly AppDbContext _dbContext;

    public NotificationRepoImpl(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Persist a notification row.
    /// </summary>
    public async Task AddAsync(
        Models.Notification notification,
        CancellationToken ct = default)
    {
        await _dbContext.Notifications.AddAsync(notification, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Mark a notification as read.
    /// </summary>
    public async Task UpdateStatusAsync(
        Guid notificationId,
        CancellationToken ct = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

        if (notification == null)
            return;

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<Models.Notification?> GetByIdAsync(
        Guid notificationId,
        CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);
    }

    // ── Worker recipient queries ───────────────────────────────────────────

    public async Task<List<Models.Notification>> GetAllByWorkerIdAsync(
        Guid workerId,
        CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientWorkerId == workerId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllUnreadByWorkerIdAsync(
        Guid workerId,
        CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientWorkerId == workerId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllReadByWorkerIdAsync(
        Guid workerId,
        CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientWorkerId == workerId && n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IList<NotificationFormatComponent>> GetFormattedWorkerNotificationsAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct = default)
    {
        var rows = await (
            from n in _dbContext.Notifications
            where n.RecipientWorkerId == workerId
                  && (!isRead.HasValue || n.IsRead == isRead.Value)
            join req in _dbContext.Requests on n.RequestId equals req.Id into requestGroup
            from req in requestGroup.DefaultIfEmpty()
            join emp in _dbContext.Employers on req.EmployerId equals emp.Id into employerGroup
            from emp in employerGroup.DefaultIfEmpty()
            orderby n.CreatedAt descending
            select new
            {
                n.Id,
                n.RequestId,
                EmployerName = (string?)emp.Name,
                WorkerName = (string?)null,
                n.Type,
                n.CreatedAt
            }
        ).ToListAsync(ct);

        var requestIds = rows
            .Where(r => r.RequestId.HasValue)
            .Select(r => r.RequestId!.Value)
            .Distinct()
            .ToList();
        var descByRequest = await GetCategoryFieldsByRequestIdAsync(requestIds, ct);

        return rows.Select(r => new NotificationFormatComponent(
            r.Id,
            r.EmployerName,
            r.WorkerName,
            r.Type,
            r.RequestId.HasValue && descByRequest.TryGetValue(r.RequestId.Value, out var desc)
                ? desc
                : null,
            r.CreatedAt
        )).ToList();
    }

    // ── Employer recipient queries ─────────────────────────────────────────

    public async Task<List<Models.Notification>> GetAllByEmployerIdAsync(
        Guid employerId,
        CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientEmployerId == employerId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllUnreadByEmployerIdAsync(
        Guid employerId,
        CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientEmployerId == employerId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Models.Notification>> GetAllReadByEmployerIdAsync(
        Guid employerId,
        CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.RecipientEmployerId == employerId && n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IList<NotificationFormatComponent>> GetFormattedEmployerNotificationsAsync(
        Guid employerId,
        bool? isRead,
        CancellationToken ct = default)
    {
        var rows = await (
            from n in _dbContext.Notifications
            where n.RecipientEmployerId == employerId
                  && (!isRead.HasValue || n.IsRead == isRead.Value)
            join req in _dbContext.Requests on n.RequestId equals req.Id into requestGroup
            from req in requestGroup.DefaultIfEmpty()
            join worker in _dbContext.Workers on req.WorkerId equals worker.Id into workerGroup
            from worker in workerGroup.DefaultIfEmpty()
            join emp in _dbContext.Employers on n.RecipientEmployerId equals emp.Id into employerGroup
            from emp in employerGroup.DefaultIfEmpty()
            orderby n.CreatedAt descending
            select new
            {
                n.Id,
                n.RequestId,
                EmployerName = (string?)emp.Name,
                WorkerName = (string?)worker.Name,
                n.Type,
                n.CreatedAt
            }
        ).ToListAsync(ct);

        var requestIds = rows
            .Where(r => r.RequestId.HasValue)
            .Select(r => r.RequestId!.Value)
            .Distinct()
            .ToList();
        var descByRequest = await GetCategoryFieldsByRequestIdAsync(requestIds, ct);

        return rows.Select(r => new NotificationFormatComponent(
            r.Id,
            r.EmployerName,
            r.WorkerName,
            r.Type,
            r.RequestId.HasValue && descByRequest.TryGetValue(r.RequestId.Value, out var desc)
                ? desc
                : null,
            r.CreatedAt
        )).ToList();
    }

    /// <summary>
    /// For each requestId, build a display string of categories with their fields,
    /// e.g. "Personal Information (Phone, Address), Medical Information (Vaccination Records)".
    /// Returns an empty dictionary when no input.
    /// </summary>
    private async Task<Dictionary<Guid, string>> GetCategoryFieldsByRequestIdAsync(
        List<Guid> requestIds,
        CancellationToken ct)
    {
        if (requestIds.Count == 0) return new Dictionary<Guid, string>();

        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .Where(p => requestIds.Contains(p.RequestId))
            .Include(p => p.Field).ThenInclude(f => f!.Category)
            .Include(p => p.WorkerInfo).ThenInclude(wi => wi!.Field).ThenInclude(f => f!.Category)
            .ToListAsync(ct);

        return permissions
            .GroupBy(p => p.RequestId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var byCategory = g
                        .Select(p => new
                        {
                            Category = p.Field?.Category?.CategoryName
                                       ?? p.WorkerInfo?.Field?.Category?.CategoryName
                                       ?? "OtherInformation",
                            Label = p.Field?.Label
                                    ?? p.WorkerInfo?.CustomLabel
                                    ?? p.WorkerInfo?.Field?.Label
                                    ?? "Unknown"
                        })
                        .GroupBy(x => x.Category);

                    return string.Join(", ",
                        byCategory.Select(cg =>
                            $"{HumanizeCategoryName(cg.Key)} ({string.Join(", ", cg.Select(x => x.Label).Distinct())})"));
                });
    }

    /// <summary>
    /// Convert a PascalCase category name like "PersonalInformation" to "Personal Information".
    /// </summary>
    private static string HumanizeCategoryName(string camelCase)
    {
        if (string.IsNullOrEmpty(camelCase)) return camelCase;
        var sb = new System.Text.StringBuilder(camelCase.Length + 4);
        for (int i = 0; i < camelCase.Length; i++)
        {
            if (i > 0 && char.IsUpper(camelCase[i])) sb.Append(' ');
            sb.Append(camelCase[i]);
        }
        return sb.ToString();
    }

    // ── Existing formatting helpers ────────────────────────────────────────

    /// <summary>
    /// Format a NotificationEvent into a human-readable DTO.
    /// Looks up employer and worker names from DB.
    /// </summary>
    public async Task<NotificationFormat> FormatNotification(
        NotificationEvent e,
        CancellationToken ct = default)
    {
        var employerName = (await _dbContext.Employers
            .FirstOrDefaultAsync(emp => emp.Id == e.EmployerId, ct))?.Name;

        var workerName = (await _dbContext.Workers
            .FirstOrDefaultAsync(w => w.Id == e.WorkerId, ct))?.Name;

        // Prefer explicit FieldLabel from the event; otherwise derive a
        // category-fields description from the request so the live push has
        // meaningful content (matches the bell/list rendering).
        string? desc = e.FieldLabel;
        if (string.IsNullOrEmpty(desc) && e.RequestId.HasValue)
        {
            var descMap = await GetCategoryFieldsByRequestIdAsync(
                new List<Guid> { e.RequestId.Value }, ct);
            descMap.TryGetValue(e.RequestId.Value, out desc);
        }

        return new NotificationFormat(
            employerName,
            workerName,
            e.Type.ToString(),
            desc,
            e.CreateAt
        );
    }

    /// <summary>
    /// Format a persisted Notification row into a display DTO.
    /// Works for both worker and employer recipients.
    /// </summary>
    public async Task<NotificationFormatComponent> FormatNotificationPipeline(
        Models.Notification n,
        CancellationToken ct = default)
    {
        var request = n.RequestId.HasValue
            ? await _dbContext.Requests.FirstOrDefaultAsync(r => r.Id == n.RequestId, ct)
            : null;

        string? employerName = null;
        string? workerName = null;

        if (request != null)
        {
            employerName = (await _dbContext.Employers
                .FirstOrDefaultAsync(e => e.Id == request.EmployerId, ct))?.Name;

            workerName = (await _dbContext.Workers
                .FirstOrDefaultAsync(w => w.Id == request.WorkerId, ct))?.Name;
        }

        return new NotificationFormatComponent(
            n.Id,
            employerName,
            workerName,
            n.Type,
            null,
            n.CreatedAt
        );
    }

    /// <summary>
    /// Legacy worker-only formatted query.
    /// </summary>
    public async Task<IList<NotificationFormatComponent>> GetFormattedNotificationsAsync(
        Guid workerId,
        bool? isRead,
        CancellationToken ct = default)
    {
        return await GetFormattedWorkerNotificationsAsync(workerId, isRead, ct);
    }
}
