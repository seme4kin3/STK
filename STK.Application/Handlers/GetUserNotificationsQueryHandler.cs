using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.DTOs;
using STK.Application.Queries;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, List<NotificationDto>>
    {
        private readonly DataContext _context;

        public GetUserNotificationsQueryHandler(DataContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == request.UserId)
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Message = n.Message,
                    CreatedDate = n.CreatedDate,
                    IsRead = n.IsRead,
                    OrgId = n.RelatedOrganizationId,
                    TableName = n.TableName
                })
                .ToListAsync(cancellationToken);

            return notifications;
        }
    }
}
