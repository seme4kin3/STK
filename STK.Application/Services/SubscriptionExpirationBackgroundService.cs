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
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12); // Тест: каждые 30 сек

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

                //Ждём до следующей полуночи (локальное время)
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1); // следующая полночь
                var delay = nextRun - now;

                _logger.LogInformation("Next check scheduled at {NextRun}", nextRun);

                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task ProcessExpiredUsers(DataContext context, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow; // <--- важно! не .Date

            var expiredUsers = await context.Users
                .Include(u => u.UserRoles)
                .Where(u => u.IsActive &&
                            u.SubscriptionEndTime.HasValue &&
                            u.SubscriptionEndTime.Value < now)
                .ToListAsync(cancellationToken);

            if (!expiredUsers.Any())
            {
                _logger.LogInformation("No expired users found at {Time}", now);
                return;
            }

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

                _logger.LogInformation("User {UserId} deactivated due to expired subscription (ended {EndTime})", user.Id, user.SubscriptionEndTime);
            }

            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Deactivated {Count} users due to expired subscription at {Time}", expiredUsers.Count, now);
        }
    }
}

