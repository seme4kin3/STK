using Microsoft.AspNetCore.SignalR;
using STK.Application.Services;
using System.Collections.Concurrent;

namespace STK.API.SignalR
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<ConnectionManager> _logger;

        public ConnectionManager(
            IHubContext<NotificationHub> hubContext,
            ILogger<ConnectionManager> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public Task AddConnection(Guid userId, string connectionId)
        {
            _userConnections.AddOrUpdate(
                userId,
                new HashSet<string> { connectionId },
                (_, connections) =>
                {
                    connections.Add(connectionId);
                    return connections;
                });

            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, connectionId);
            return Task.CompletedTask;
        }

        public Task RemoveConnection(Guid userId, string connectionId)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);

                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }

            _logger.LogInformation("User {UserId} disconnected with connection {ConnectionId}", userId, connectionId);
            return Task.CompletedTask;
        }

        public async Task SendNotificationAsync(Guid userId, NotificationMessage message)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                var tasks = connections.Select(
                    connectionId => _hubContext.Clients.Client(connectionId)
                        .SendAsync("ReceiveNotification", message));

                await Task.WhenAll(tasks);
                _logger.LogDebug("Notification sent to user {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("User {UserId} has no active connections", userId);
            }
        }

        public async Task SendNotificationToAllAsync(NotificationMessage message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
            _logger.LogDebug("Notification broadcasted to all connected clients");
        }

        public IEnumerable<string> GetUserConnections(Guid userId)
        {
            return _userConnections.TryGetValue(userId, out var connections)
                ? connections
                : Enumerable.Empty<string>();
        }

        public int GetConnectedUsersCount()
        {
            return _userConnections.Count;
        }
    }
}
