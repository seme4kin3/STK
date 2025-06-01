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

        public NotificationHub(IConnectionManager connectionManager, IMediator mediator)
        {
            _connectionManager = connectionManager;
            _mediator = mediator;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            await _connectionManager.AddConnection(userId, Context.ConnectionId);

            // Отправляем непрочитанные уведомления при подключении
            var unreadNotifications = await _mediator.Send(new GetUserNotificationsQuery(userId));
            foreach (var notification in unreadNotifications.Where(n => !n.IsRead))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveNotification",
                    new NotificationMessage(
                        "Новое уведомление",
                        notification.Message,
                        notification.CreatedDate));
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = GetUserId();
            await _connectionManager.RemoveConnection(userId, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        private Guid GetUserId()
        {
            return Guid.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}
