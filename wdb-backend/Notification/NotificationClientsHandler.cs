using MediatR;
using Microsoft.AspNetCore.SignalR;
using wdb_backend.Abstractions;
using wdb_backend.DTOs;

namespace wdb_backend.Notification;

/// <summary>
/// Handles NotificationEvent: persists to DB then pushes a formatted
/// message to the target worker's SignalR group.
/// </summary>
public class NotificationClientsHandler : INotificationHandler<NotificationEvent>
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly INotificationRepository _notificationRepo;

    public NotificationClientsHandler(
        IHubContext<NotificationsHub> hubContext,
        INotificationRepository notificationRepo)
    {
        _hubContext = hubContext;
        _notificationRepo = notificationRepo;
    }

    public async Task Handle(NotificationEvent e, CancellationToken ct)
    {
        // Persist the notification row using the new schema (single-recipient pattern).
        await _notificationRepo.AddAsync(new Models.Notification
        {
            RecipientWorkerId = e.WorkerId,     // worker is the recipient
            Type = e.Type,
            RequestId = e.RequestId,
            IsRead = false
        }, ct);

        // Push structured payload so the frontend can render the toast with the
        // same icon + label + categories layout as the bell dropdown.
        var format = await _notificationRepo.FormatNotification(e, ct);
        var payload = new
        {
            type = format.NotificationType,
            employerName = format.EmployerName,
            workerInfoDesc = format.WorkInfoDesc
        };
        await _hubContext.Clients
            .Group(e.WorkerId.ToString())
            .SendAsync("NotificationInfo", payload, ct);
    }
}
