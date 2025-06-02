using MediatR;

namespace STK.Application.Commands
{
    public record MarkNotificationAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<Unit>;
}
