using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using STK.Application.Commands;
using STK.Persistance;
using System.Text.Json;

namespace STK.Application.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);

        public NotificationBackgroundService(
            IServiceProvider services,
            ILogger<NotificationBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Background Service is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                        await CheckChangesAndNotify(mediator, context, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing notification check.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CheckChangesAndNotify(IMediator mediator, DataContext context, CancellationToken cancellationToken)
        {
            var users = await context.Users.ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                await ProcessUserChanges(mediator, context, user.Id, cancellationToken);
            }
        }

        private async Task ProcessUserChanges(IMediator mediator, DataContext context, Guid userId, CancellationToken cancellationToken)
        {
            var lastCheckTime = DateTime.UtcNow.AddHours(-1);

            // Получаем изменения организаций
            var orgChanges = await GetOrganizationChanges(context, userId, lastCheckTime, cancellationToken);
            foreach (var org in orgChanges)
            {
                await mediator.Send(new SendNotificationCommand(
                    userId,
                    $"Изменение в организации",
                    org.FullName),
                    cancellationToken);
            }

            // Получаем изменения сертификатов
            var certChanges = await GetCertificateChanges(context, userId, lastCheckTime, cancellationToken);
            foreach (var cert in certChanges)
            {
                await mediator.Send(new SendNotificationCommand(
                    userId,
                    $"Изменение в сертификате",
                    cert.Title),
                    cancellationToken);
            }
        }

        private async Task<List<NotificationOrgDto>> GetOrganizationChanges(
            DataContext context,
            Guid userId,
            DateTime since,
            CancellationToken cancellationToken)
        {
            var orgIds = await context.UsersFavoritesOrganizations
                .Where(ufo => ufo.UserId == userId)
                .Select(ufo => ufo.OrganizationId)
                .ToListAsync(cancellationToken);

            if (!orgIds.Any())
                return new List<NotificationOrgDto>();

            var auditLogs = await context.AuditLog
               .Where(a => a.ChangedAt >= since &&
                          a.TableName == "Organizations" &&
                          orgIds.Contains(a.RecordId))
               .OrderByDescending(a => a.ChangedAt)
               .ToListAsync(cancellationToken);

            return auditLogs
                .GroupBy(a => a.RecordId)
                .Select(g => g.First())
                .Select(a => new NotificationOrgDto
                {
                    Id = a.RecordId,
                    FullName = ExtractNameFromJson(a.NewData ?? a.OldData)
                })
                .ToList();
        }

        private async Task<List<NotificationCertDto>> GetCertificateChanges(
            DataContext context,
            Guid userId,
            DateTime since,
            CancellationToken cancellationToken)
        {
            var certIds = await context.UsersFavoritesCertificates
                .Where(ufc => ufc.UserId == userId)
                .Select(ufc => ufc.CertificateId)
                .ToListAsync(cancellationToken);

            if (!certIds.Any())
                return new List<NotificationCertDto>();

            var auditLogs = await context.AuditLog
               .Where(a => a.ChangedAt >= since &&
                          a.TableName == "Certificates" &&
                          certIds.Contains(a.RecordId))
               .OrderByDescending(a => a.ChangedAt)
               .ToListAsync(cancellationToken);

            return auditLogs
                .GroupBy(a => a.RecordId)
                .Select(g => g.First())
                .Select(a => new NotificationCertDto
                {
                    Id = a.RecordId,
                    Title = ExtractNameFromJson(a.NewData ?? a.OldData)
                })
                .ToList();
        }

        private string ExtractNameFromJson(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
                return "Unknown";

            try
            {
                using var doc = JsonDocument.Parse(jsonData);
                if (doc.RootElement.TryGetProperty("FullName", out var nameProp) &&
                    nameProp.ValueKind == JsonValueKind.String)
                {
                    return nameProp.GetString() ?? "Unknown";
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }

    // DTO классы
    public class NotificationOrgDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
    }

    public class NotificationCertDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }
}
