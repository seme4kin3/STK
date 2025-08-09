using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using STK.Domain.Entities;
using STK.Persistance;

namespace STK.Application.Services
{
    /// <summary>
    /// Background service that deactivates expired user accounts once per day.
    /// </summary>
    public class SubscriptionExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<SubscriptionExpirationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

        public SubscriptionExpirationBackgroundService(
        IServiceProvider services,
        ILogger<SubscriptionExpirationBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscription expiration service is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                    await ProcessExpiredUsers(context, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing subscription expiration check.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessExpiredUsers(DataContext context, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var expiredUsers = await context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.IsActive && u.SubscriptionEndTime.HasValue && u.SubscriptionEndTime < now)
                .ToListAsync(cancellationToken);

            if (!expiredUsers.Any())
                return;

            var freeRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "free", cancellationToken);
            if (freeRole == null)
            {
                _logger.LogWarning("Role 'free' not found in database. Skipping expiration processing.");
                return;
            }

            foreach (var user in expiredUsers)
            {
                user.IsActive = false;
                user.CountRequestAI = 0;
                context.UserRoles.RemoveRange(user.UserRoles);
                context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = freeRole.Id });
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Deactivated {Count} users due to expired subscription", expiredUsers.Count);
        }
    }
}

