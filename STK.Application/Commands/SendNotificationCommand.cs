using MediatR;
using STK.Domain.Entities;

namespace STK.Application.Commands
{
    public record SendNotificationCommand(Guid UserId, string Title,
        string Content, string TableName, Guid? OrgId) : IRequest<Notification>;
}
