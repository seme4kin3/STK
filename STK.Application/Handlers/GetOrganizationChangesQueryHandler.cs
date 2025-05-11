using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs;
using STK.Application.Queries;
using STK.Persistance;
using System.Text.Json;

namespace STK.Application.Handlers
{
    public class GetOrganizationChangesQueryHandler : IRequestHandler<GetOrganizationChangesQuery, IReadOnlyList<AuditLogDto>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetOrganizationChangesQueryHandler> _logger;

        public GetOrganizationChangesQueryHandler(DataContext dataContext, ILogger<GetOrganizationChangesQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<AuditLogDto>> Handle(GetOrganizationChangesQuery query, CancellationToken cancellationToken)
        {
            try
            {
                if (query.OrganizationId == Guid.Empty)
                    throw new ArgumentException("OrganizationId cannot be empty.", nameof(query.OrganizationId));

                // Фиксируем текущее время для стабильного запроса
                var now = DateTime.UtcNow;
                var dateFrom = now.Date.AddMonths(-1);

                // Определяем допустимые таблицы
                var validTableNames = new[] { "Organizations", "Requisites", "Managements", "Certificates" };

                // Выполняем запрос к базе данных
                var changes = await _dataContext.AuditLog
                    .AsNoTracking()
                    .Where(log => validTableNames.Contains(log.TableName))
                    .Where(log => log.Operation == "UPDATE")
                    .Where(log => log.ChangedAt >= dateFrom && log.ChangedAt <= now)
                    .Where(log => log.TableName == "Organizations"
                        ? log.RecordId == query.OrganizationId
                        : log.RelatedOrganizationId != null && log.RelatedOrganizationId == query.OrganizationId)
                    .Select(log => new
                    {
                        log.Id,
                        log.TableName,
                        log.RecordId,
                        log.RelatedOrganizationId,
                        log.Operation,
                        log.OldData,
                        log.NewData,
                        log.ChangedAt
                    })
                    .ToListAsync(cancellationToken);

                // Группировка и выбор последней записи для каждой таблицы на клиентской стороне
                var groupedChanges = changes
                    .GroupBy(log => log.TableName)
                    .Select(g => g.OrderByDescending(log => log.ChangedAt).FirstOrDefault())
                    .Where(log => log != null)
                    .ToList();

                // Маппинг в DTO с десериализацией
                var dtos = groupedChanges
                    .Select(log => new AuditLogDto
                    {
                        Id = log.Id,
                        TableName = log.TableName,
                        RecordId = log.RecordId,
                        RelatedOrganizationId = log.RelatedOrganizationId,
                        Operation = log.Operation,
                        OldData = log.OldData != null ? JsonSerializer.Deserialize<object>(log.OldData) : null,
                        NewData = log.NewData != null ? JsonSerializer.Deserialize<object>(log.NewData) : null,
                        ChangedAt = log.ChangedAt
                    })
                    .ToList();

                return dtos.AsReadOnly();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process GetOrganizationChangesQuery for OrganizationId {OrganizationId}. Error: {Message}",
                    query.OrganizationId, ex.Message);
                throw new ApplicationException("An error occurred while retrieving organization changes.", ex);
            }
        }
    }
}
