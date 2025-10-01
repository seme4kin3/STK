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
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(60);
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

            // Update LastChangedAtDate for all organizations
            await UpdateLastChangedAtDateForAllOrganizations(context, _lastCheckTime, cancellationToken);

            // Process notifications for users
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
                var changedOrganizations = await GetAllChangedOrganizations(context, lastCheckTime, cancellationToken);

                if (changedOrganizations.Any())
                {
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

                        //org.LastChangedAtDate = latestChange.ChangedAt;
                        org.LastChangedAtDate = DateTime.SpecifyKind(latestChange.ChangedAt, DateTimeKind.Utc);
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
            // Изменения в Organizations
            var organizationAuditLogs = await context.AuditLog
                .Where(a => a.ChangedAt >= since && a.TableName == "Organizations")
                .Select(a => new OrganizationChangeInfo
                {
                    OrganizationId = a.RecordId,
                    ChangedAt = a.ChangedAt,
                    TableName = a.TableName
                })
                .ToListAsync(cancellationToken);

            // Изменения в связанных таблицах (включая BankruptcyIntentions)
            var relatedTables = new[] { "Requisites", "Managements", "Stamps", "ArbitrationsCases", "BankruptcyIntentions" };

            var relatedAuditLogs = await context.AuditLog
                .Where(a => a.ChangedAt >= since &&
                            relatedTables.Contains(a.TableName) &&
                            a.RelatedOrganizationId.HasValue)
                .Select(a => new OrganizationChangeInfo
                {
                    OrganizationId = a.RelatedOrganizationId!.Value,
                    ChangedAt = a.ChangedAt,
                    TableName = a.TableName
                })
                .ToListAsync(cancellationToken);

            return organizationAuditLogs.Concat(relatedAuditLogs).ToList();
        }

        private async Task ProcessUserChanges(IMediator mediator, DataContext context, Guid userId, DateTime lastCheckTime, CancellationToken cancellationToken)
        {
            try
            {
                // Organization changes (включая изменения в BankruptcyIntentions)
                var orgChanges = await GetOrganizationChanges(context, userId, lastCheckTime, cancellationToken);
                foreach (var org in orgChanges)
                {
                    // таблица с самой свежей датой изменения
                    var latestTable = org.ChangedTables.FirstOrDefault() ?? "Organizations";

                    // подбираем специализированный заголовок
                    string title = latestTable switch
                    {
                        "Organizations" => "Изменение в организации",
                        "Requisites" => "Изменение в реквизитах организации",
                        "Managements" => "Изменение в руководстве организации",
                        "Stamps" => "Изменение в клеммах организации",
                        "ArbitrationsCases" => "Изменение в судебных делах организации",
                        "Certificates" => "Изменение в сертификате организации",
                        "BankruptcyIntentions" => "Изменение в намерениях банкротства организации",
                        _ => $"Изменение в {latestTable}"
                    };

                    // детали — список всех таблиц
                    var details = org.ChangedTables.Any()
                        ? $"Детали: изменения в таблиц(е/ах): {string.Join(", ", org.ChangedTables)}"
                        : "Детали: таблицы изменений не определены";

                    await mediator.Send(new SendNotificationCommand(
                        userId,
                        title,
                        $"{org.FullName}\n{details}",
                        "Организация",
                        org.Id),
                        cancellationToken);
                }

                // Certificate changes (как было)
                var certChanges = await GetCertificateChanges(context, userId, lastCheckTime, cancellationToken);
                foreach (var cert in certChanges)
                {
                    await mediator.Send(new SendNotificationCommand(
                        userId,
                        "Изменение в сертификате",
                        cert.Title,
                        "Сертификат",
                        cert.OrgId),
                        cancellationToken);
                }

                if (orgChanges.Any() || certChanges.Any())
                {
                    _logger.LogInformation(
                        "Processed {OrgCount} org changes, {CertCount} cert changes, changes for user {UserId}",
                        orgChanges.Count, certChanges.Count, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing changes for user {UserId}", userId);
            }
        }

        private async Task<List<NotificationOrgWithDetailsDto>> GetOrganizationChanges(
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
                return new List<NotificationOrgWithDetailsDto>();

            // Таблицы организации + связанные (добавили BankruptcyIntentions)
            var relatedTables = new[] { "Requisites", "Managements", "Stamps", "ArbitrationsCases", "BankruptcyIntentions" };

            var organizationAuditLogs = await context.AuditLog
                .Where(a => a.ChangedAt >= since &&
                            a.TableName == "Organizations" &&
                            orgIds.Contains(a.RecordId))
                .ToListAsync(cancellationToken);

            var relatedAuditLogs = await context.AuditLog
                .Where(a => a.ChangedAt >= since &&
                            relatedTables.Contains(a.TableName) &&
                            a.RelatedOrganizationId.HasValue &&
                            orgIds.Contains(a.RelatedOrganizationId.Value))
                .ToListAsync(cancellationToken);

            var allAuditLogs = organizationAuditLogs.Concat(relatedAuditLogs);

            // Группируем по организации, собираем список таблиц и последнюю дату
            var grouped = allAuditLogs
                .GroupBy(a => a.TableName == "Organizations" ? a.RecordId : a.RelatedOrganizationId!.Value)
                .Select(g => new
                {
                    OrganizationId = g.Key,
                    LatestChangeAt = g.Max(x => x.ChangedAt),
                    Tables = g.Select(x => x.TableName).Distinct().OrderBy(t => t).ToList()
                })
                .ToList();

            var names = await context.Organizations
                .Where(o => grouped.Select(x => x.OrganizationId).Contains(o.Id))
                .Select(o => new { o.Id, o.FullName })
                .ToDictionaryAsync(o => o.Id, o => o.FullName, cancellationToken);

            return grouped
                .Select(g => new NotificationOrgWithDetailsDto
                {
                    Id = g.OrganizationId,
                    FullName = names.TryGetValue(g.OrganizationId, out var name) ? name : "Неизвестная организация",
                    LatestChangeAt = g.LatestChangeAt,
                    ChangedTables = g.Tables
                })
                .OrderByDescending(x => x.LatestChangeAt)
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
    public class NotificationOrgWithDetailsDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = "Неизвестная организация";
        public DateTime LatestChangeAt { get; set; }
        public List<string> ChangedTables { get; set; } = new();
    }

    public class NotificationCertDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid? OrgId { get; set; }
    }

}
