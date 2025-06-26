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
    //public class NotificationBackgroundService : BackgroundService
    //{
    //    private readonly IServiceProvider _services;
    //    private readonly ILogger<NotificationBackgroundService> _logger;
    //    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30); // Уменьшил интервал для тестирования
    //    private DateTime _lastCheckTime = DateTime.UtcNow;

    //    public NotificationBackgroundService(
    //        IServiceProvider services,
    //        ILogger<NotificationBackgroundService> logger)
    //    {
    //        _services = services;
    //        _logger = logger;
    //    }

    //    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    //    {
    //        _logger.LogInformation("Notification Background Service is running.");

    //        while (!stoppingToken.IsCancellationRequested)
    //        {
    //            try
    //            {
    //                using (var scope = _services.CreateScope())
    //                {
    //                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    //                    var context = scope.ServiceProvider.GetRequiredService<DataContext>();

    //                    await CheckChangesAndNotify(mediator, context, stoppingToken);
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                _logger.LogError(ex, "Error occurred executing notification check.");
    //            }

    //            await Task.Delay(_checkInterval, stoppingToken);
    //        }
    //    }

    //    private async Task CheckChangesAndNotify(IMediator mediator, DataContext context, CancellationToken cancellationToken)
    //    {
    //        var currentCheckTime = DateTime.UtcNow;

    //        _logger.LogInformation("Checking for changes since {LastCheckTime}", _lastCheckTime);

    //        var users = await context.Users.ToListAsync(cancellationToken);

    //        foreach (var user in users)
    //        {
    //            await ProcessUserChanges(mediator, context, user.Id, _lastCheckTime, cancellationToken);
    //        }

    //        _lastCheckTime = currentCheckTime;
    //    }

    //    private async Task ProcessUserChanges(IMediator mediator, DataContext context, Guid userId, DateTime lastCheckTime, CancellationToken cancellationToken)
    //    {
    //        try
    //        {
    //            // Получаем изменения организаций
    //            var orgChanges = await GetOrganizationChanges(context, userId, lastCheckTime, cancellationToken);
    //            foreach (var org in orgChanges)
    //            {
    //                await mediator.Send(new SendNotificationCommand(
    //                    userId,
    //                    "Изменение в организации",
    //                    org.FullName, "Организация", org.Id),
    //                    cancellationToken);
    //            }

    //            // Получаем изменения сертификатов
    //            var certChanges = await GetCertificateChanges(context, userId, lastCheckTime, cancellationToken);
    //            foreach (var cert in certChanges)
    //            {
    //                await mediator.Send(new SendNotificationCommand(
    //                    userId,
    //                    "Изменение в сертификате",
    //                    cert.Title, "Сертификат", cert.OrgId),
    //                    cancellationToken);
    //            }

    //            if (orgChanges.Any() || certChanges.Any())
    //            {
    //                _logger.LogInformation("Processed {OrgCount} org changes and {CertCount} cert changes for user {UserId}",
    //                    orgChanges.Count, certChanges.Count, userId);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error processing changes for user {UserId}", userId);
    //        }
    //    }

    //    private async Task<List<NotificationOrgDto>> GetOrganizationChanges(
    //        DataContext context,
    //        Guid userId,
    //        DateTime since,
    //        CancellationToken cancellationToken)
    //    {
    //        var orgIds = await context.UsersFavoritesOrganizations
    //            .Where(ufo => ufo.UserId == userId)
    //            .Select(ufo => ufo.OrganizationId)
    //            .ToListAsync(cancellationToken);

    //        if (!orgIds.Any())
    //            return new List<NotificationOrgDto>();

    //        // Получаем изменения в основной таблице Organizations
    //        var organizationAuditLogs = await context.AuditLog
    //            .Where(a => a.ChangedAt >= since &&
    //                       a.TableName == "Organizations" &&
    //                       orgIds.Contains(a.RecordId))
    //            .ToListAsync(cancellationToken);

    //        // Получаем изменения в связанных таблицах Requisites и Managements
    //        var relatedAuditLogs = await context.AuditLog
    //            .Where(a => a.ChangedAt >= since &&
    //                       (a.TableName == "Requisites" || a.TableName == "Managements") &&
    //                       a.RelatedOrganizationId.HasValue &&
    //                       orgIds.Contains(a.RelatedOrganizationId.Value))
    //            .ToListAsync(cancellationToken);

    //        // Объединяем все логи изменений
    //        var allAuditLogs = organizationAuditLogs.Concat(relatedAuditLogs);

    //        // Группируем по организации (используем RecordId для Organizations и RelatedOrgId для связанных таблиц)
    //        var groupedLogs = allAuditLogs
    //            .GroupBy(a => a.TableName == "Organizations" ? a.RecordId : a.RelatedOrganizationId.Value)
    //            .Select(g => new
    //            {
    //                OrganizationId = g.Key,
    //                LatestChange = g.OrderByDescending(a => a.ChangedAt).First()
    //            })
    //            .ToList();

    //        // Получаем названия организаций
    //        var organizationNames = await context.Organizations
    //            .Where(o => groupedLogs.Select(gl => gl.OrganizationId).Contains(o.Id))
    //            .Select(o => new { o.Id, o.FullName })
    //            .ToDictionaryAsync(o => o.Id, o => o.FullName, cancellationToken);

    //        return groupedLogs
    //            .Select(gl => new NotificationOrgDto
    //            {
    //                Id = gl.OrganizationId,
    //                FullName = organizationNames.TryGetValue(gl.OrganizationId, out var name)
    //                    ? name
    //                    : "Неизвестная организация"
    //            })
    //            .OrderByDescending(n => groupedLogs.First(gl => gl.OrganizationId == n.Id).LatestChange.ChangedAt)
    //            .ToList();
    //    }

    //    private async Task<List<NotificationCertDto>> GetCertificateChanges(
    //        DataContext context,
    //        Guid userId,
    //        DateTime since,
    //        CancellationToken cancellationToken)
    //    {
    //        var certIds = await context.UsersFavoritesCertificates
    //            .Where(ufc => ufc.UserId == userId)
    //            .Select(ufc => ufc.CertificateId)
    //            .ToListAsync(cancellationToken);

    //        if (!certIds.Any())
    //            return new List<NotificationCertDto>();

    //        var auditLogs = await context.AuditLog
    //           .Where(a => a.ChangedAt >= since &&
    //                      a.TableName == "Certificates" &&
    //                      certIds.Contains(a.RecordId))
    //           .OrderByDescending(a => a.ChangedAt)
    //           .ToListAsync(cancellationToken);

    //        return auditLogs
    //            .GroupBy(a => a.RecordId)
    //            .Select(g => g.First())
    //            .Select(a => new NotificationCertDto
    //            {
    //                Id = a.RecordId,
    //                Title = ExtractFieldFromJson(a.NewData ?? a.OldData, "Title") ?? "Неизвестный сертификат",
    //                OrgId = a.RelatedOrganizationId
    //            })
    //            .ToList();
    //    }

    //    private string ExtractFieldFromJson(string jsonData, string fieldName)
    //    {
    //        if (string.IsNullOrEmpty(jsonData))
    //            return null;

    //        try
    //        {
    //            using var doc = JsonDocument.Parse(jsonData);
    //            if (doc.RootElement.TryGetProperty(fieldName, out var prop) &&
    //                prop.ValueKind == JsonValueKind.String)
    //            {
    //                return prop.GetString();
    //            }
    //            return null;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogWarning(ex, "Failed to extract {FieldName} from JSON: {JsonData}", fieldName, jsonData);
    //            return null;
    //        }
    //    }
    //}

    // DTO классы

    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(10); // Уменьшил интервал для тестирования
        private DateTime _lastCheckTime = DateTime.UtcNow;

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
            var currentCheckTime = DateTime.UtcNow;

            _logger.LogInformation("Checking for changes since {LastCheckTime}", _lastCheckTime);

            // Сначала обновляем LastChangedAtDate для всех измененных организаций
            await UpdateLastChangedAtDateForAllOrganizations(context, _lastCheckTime, cancellationToken);

            // Затем обрабатываем уведомления для пользователей
            var users = await context.Users.ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                await ProcessUserChanges(mediator, context, user.Id, _lastCheckTime, cancellationToken);
            }

            _lastCheckTime = currentCheckTime;
        }

        private async Task UpdateLastChangedAtDateForAllOrganizations(DataContext context, DateTime lastCheckTime, CancellationToken cancellationToken)
        {
            try
            {
                // Получаем все организации, которые изменились с последней проверки
                var changedOrganizations = await GetAllChangedOrganizations(context, lastCheckTime, cancellationToken);

                if (changedOrganizations.Any())
                {
                    // Обновляем LastChangedAtDate для каждой измененной организации
                    var organizationIds = changedOrganizations.Select(co => co.OrganizationId).ToList();

                    var organizations = await context.Organizations
                        .Where(o => organizationIds.Contains(o.Id))
                        .ToListAsync(cancellationToken);

                    foreach (var org in organizations)
                    {
                        var latestChange = changedOrganizations
                            .Where(co => co.OrganizationId == org.Id)
                            .OrderByDescending(co => co.ChangedAt)
                            .First();

                        org.LastChangedAtDate = latestChange.ChangedAt;
                    }

                    await context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Updated LastChangedAtDate for {Count} organizations", organizations.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating LastChangedAtDate for organizations");
            }
        }

        private async Task<List<OrganizationChangeInfo>> GetAllChangedOrganizations(
            DataContext context,
            DateTime since,
            CancellationToken cancellationToken)
        {
            // Получаем изменения в основной таблице Organizations
            var organizationAuditLogs = await context.AuditLog
                .Where(a => a.ChangedAt >= since && a.TableName == "Organizations")
                .Select(a => new OrganizationChangeInfo
                {
                    OrganizationId = a.RecordId,
                    ChangedAt = a.ChangedAt,
                    TableName = a.TableName
                })
                .ToListAsync(cancellationToken);

            // Получаем изменения в связанных таблицах Requisites и Managements
            var relatedAuditLogs = await context.AuditLog
                .Where(a => a.ChangedAt >= since &&
                           (a.TableName == "Requisites" || a.TableName == "Managements") &&
                           a.RelatedOrganizationId.HasValue)
                .Select(a => new OrganizationChangeInfo
                {
                    OrganizationId = a.RelatedOrganizationId.Value,
                    ChangedAt = a.ChangedAt,
                    TableName = a.TableName
                })
                .ToListAsync(cancellationToken);

            // Объединяем все изменения
            return organizationAuditLogs.Concat(relatedAuditLogs).ToList();
        }

        private async Task ProcessUserChanges(IMediator mediator, DataContext context, Guid userId, DateTime lastCheckTime, CancellationToken cancellationToken)
        {
            try
            {
                // Получаем изменения организаций (только для избранных пользователем)
                var orgChanges = await GetOrganizationChanges(context, userId, lastCheckTime, cancellationToken);
                foreach (var org in orgChanges)
                {
                    await mediator.Send(new SendNotificationCommand(
                        userId,
                        "Изменение в организации",
                        org.FullName, "Организация", org.Id),
                        cancellationToken);
                }

                // Получаем изменения сертификатов
                var certChanges = await GetCertificateChanges(context, userId, lastCheckTime, cancellationToken);
                foreach (var cert in certChanges)
                {
                    await mediator.Send(new SendNotificationCommand(
                        userId,
                        "Изменение в сертификате",
                        cert.Title, "Сертификат", cert.OrgId),
                        cancellationToken);
                }

                if (orgChanges.Any() || certChanges.Any())
                {
                    _logger.LogInformation("Processed {OrgCount} org changes and {CertCount} cert changes for user {UserId}",
                        orgChanges.Count, certChanges.Count, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing changes for user {UserId}", userId);
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

            // Получаем изменения в основной таблице Organizations
            var organizationAuditLogs = await context.AuditLog
                .Where(a => a.ChangedAt >= since &&
                           a.TableName == "Organizations" &&
                           orgIds.Contains(a.RecordId))
                .ToListAsync(cancellationToken);

            // Получаем изменения в связанных таблицах Requisites и Managements
            var relatedAuditLogs = await context.AuditLog
                .Where(a => a.ChangedAt >= since &&
                           (a.TableName == "Requisites" || a.TableName == "Managements") &&
                           a.RelatedOrganizationId.HasValue &&
                           orgIds.Contains(a.RelatedOrganizationId.Value))
                .ToListAsync(cancellationToken);

            // Объединяем все логи изменений
            var allAuditLogs = organizationAuditLogs.Concat(relatedAuditLogs);

            // Группируем по организации (используем RecordId для Organizations и RelatedOrgId для связанных таблиц)
            var groupedLogs = allAuditLogs
                .GroupBy(a => a.TableName == "Organizations" ? a.RecordId : a.RelatedOrganizationId.Value)
                .Select(g => new
                {
                    OrganizationId = g.Key,
                    LatestChange = g.OrderByDescending(a => a.ChangedAt).First()
                })
                .ToList();

            // Получаем названия организаций
            var organizationNames = await context.Organizations
                .Where(o => groupedLogs.Select(gl => gl.OrganizationId).Contains(o.Id))
                .Select(o => new { o.Id, o.FullName })
                .ToDictionaryAsync(o => o.Id, o => o.FullName, cancellationToken);

            return groupedLogs
                .Select(gl => new NotificationOrgDto
                {
                    Id = gl.OrganizationId,
                    FullName = organizationNames.TryGetValue(gl.OrganizationId, out var name)
                        ? name
                        : "Неизвестная организация"
                })
                .OrderByDescending(n => groupedLogs.First(gl => gl.OrganizationId == n.Id).LatestChange.ChangedAt)
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
                    Title = ExtractFieldFromJson(a.NewData ?? a.OldData, "Title") ?? "Неизвестный сертификат",
                    OrgId = a.RelatedOrganizationId
                })
                .ToList();
        }

        private string ExtractFieldFromJson(string jsonData, string fieldName)
        {
            if (string.IsNullOrEmpty(jsonData))
                return null;

            try
            {
                using var doc = JsonDocument.Parse(jsonData);
                if (doc.RootElement.TryGetProperty(fieldName, out var prop) &&
                    prop.ValueKind == JsonValueKind.String)
                {
                    return prop.GetString();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract {FieldName} from JSON: {JsonData}", fieldName, jsonData);
                return null;
            }
        }
    }
    public class OrganizationChangeInfo
    {
        public Guid OrganizationId { get; set; }
        public DateTime ChangedAt { get; set; }
        public string TableName { get; set; }
    }
    public class NotificationOrgDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
    }

    public class NotificationCertDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid? OrgId { get; set; }
    }
}
