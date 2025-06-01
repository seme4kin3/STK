using MediatR;
using STK.Application.DTOs;


namespace STK.Application.Queries
{
    public record GetUserNotificationsQuery(Guid UserId) : IRequest<List<NotificationDto>>;
}
