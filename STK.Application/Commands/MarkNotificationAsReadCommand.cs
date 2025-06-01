using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Commands
{
    public record MarkNotificationAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<Unit>;
}
