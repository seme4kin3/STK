using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using STK.Application.Queries;
using STK.Application.Services;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace STK.API.SignalR
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IMediator _mediator;
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(
            IConnectionManager connectionManager,
            IMediator mediator,
            ILogger<NotificationHub> logger)
        {
            _connectionManager = connectionManager;
            _mediator = mediator;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = GetUserId();
                await _connectionManager.AddConnection(userId, Context.ConnectionId);

                // Отправляем непрочитанные уведомления при подключении
                var unreadNotifications = await _mediator.Send(new GetUserNotificationsQuery(userId));
                var unreadCount = 0;

                foreach (var notification in unreadNotifications.Where(n => !n.IsRead))
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("ReceiveNotification",
                        new NotificationMessage(
                            notification.Id,
                            "Непрочитанное уведомление",
                            notification.Message,
                            notification.CreatedDate, notification.OrgId, notification.TableName));
                    unreadCount++;
                }

                _logger.LogInformation("User {UserId} connected. Sent {UnreadCount} unread notifications", userId, unreadCount);
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userId = GetUserId();
                await _connectionManager.RemoveConnection(userId, Context.ConnectionId);
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }
        }

        private Guid GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in claims");
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new ArgumentException("Invalid User ID format");
            }

            return userId;
        }
    }
}
