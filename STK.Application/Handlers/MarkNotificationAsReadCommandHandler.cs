using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Unit>
    {
        private readonly DataContext _dataContext;

        public MarkNotificationAsReadCommandHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<Unit> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _dataContext.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId,
                    cancellationToken);

            if (notification != null)
            {
                notification.IsRead = true;
                await _dataContext.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}

