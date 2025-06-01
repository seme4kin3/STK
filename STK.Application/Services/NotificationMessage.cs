using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Services
{
    public record NotificationMessage(
        string Title,
        string Content,
        DateTime CreatedAt,
        bool IsUrgent = false,
        string NotificationType = "regular");
}
