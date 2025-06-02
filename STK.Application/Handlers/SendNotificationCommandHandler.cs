using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.Commands;
using STK.Application.Services;
using STK.Domain.Entities;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Notification>
    {
        private readonly DataContext _context;
        private readonly IConnectionManager _connectionManager;
        private readonly ILogger<SendNotificationCommandHandler> _logger;

        public SendNotificationCommandHandler(
            DataContext context,
            IConnectionManager connectionManager,
            ILogger<SendNotificationCommandHandler> logger)
        {
            _context = context;
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public async Task<Notification> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
        {
            // Создаем уведомление в базе данных
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Message = $"{request.Title}: {request.Content}",
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);

            // Отправляем в real-time через SignalR
            var notificationMessage = new NotificationMessage(
                request.Title,
                request.Content,
                notification.CreatedDate);

            await _connectionManager.SendNotificationAsync(request.UserId, notificationMessage);

            _logger.LogInformation("Notification sent to user {UserId}: {Title}", request.UserId, request.Title);

            return notification;
        }
    }
}

