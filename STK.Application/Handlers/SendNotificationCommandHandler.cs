using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
using STK.Application.Services;
using STK.Domain.Entities;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Notification>
    {
        private readonly DataContext _dataContext;
        private readonly IConnectionManager _connectionManager;

        public SendNotificationCommandHandler(DataContext dataContext, IConnectionManager connectionManager)
        {
            _dataContext = dataContext;
            _connectionManager = connectionManager;
        }

        public async Task<Notification> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = new Notification
            {
                UserId = request.UserId,
                Message = $"{request.Title}: {request.Content}",
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };

            _dataContext.Notifications.Add(notification);
            await _dataContext.SaveChangesAsync(cancellationToken);

            await _connectionManager.SendNotificationAsync(
                request.UserId,
                new NotificationMessage(request.Title, request.Content, DateTime.UtcNow));

            return notification;
        }
    }
}

