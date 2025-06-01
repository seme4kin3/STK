using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Services
{
    public interface IConnectionManager
    {
        Task AddConnection(Guid userId, string connectionId);
        Task RemoveConnection(Guid userId, string connectionId);
        Task SendNotificationAsync(Guid userId, NotificationMessage message);
        Task SendNotificationToAllAsync(NotificationMessage message);
        IEnumerable<string> GetUserConnections(Guid userId); // Изменили возвращаемый тип
        int GetConnectedUsersCount();
    }
}
