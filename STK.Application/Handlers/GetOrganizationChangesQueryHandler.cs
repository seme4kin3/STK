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
                // Получаем записи AuditLog для указанной организации с операцией UPDATE
                var changes = await _dataContext.AuditLog
                    .AsNoTracking()
                    .Where(log => new[] { "Organizations", "Requisites", "Managements", "Certificates" }.Contains(log.TableName))
                    .Where(log => log.Operation == "UPDATE")
                    .Where(log => log.ChangedAt >= DateTime.UtcNow.Date.AddMonths(-1) && log.ChangedAt <= DateTime.UtcNow)
                    .Where(log => log.TableName == "Organizations"
                        ? log.RecordId == query.OrganizationId
                        : log.RelatedOrganizationId != null && log.RelatedOrganizationId == query.OrganizationId)
                    .GroupBy(log => log.TableName)
                    .Select(g => g.OrderByDescending(log => log.ChangedAt)
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
                                  .FirstOrDefault())
                    .Where(log => log != null) 
                    .ToListAsync(cancellationToken);

                // Маппинг в DTO с десериализацией
                var dtos = changes.Select(log => new AuditLogDto
                {
                    Id = log.Id,
                    TableName = log.TableName,
                    RecordId = log.RecordId,
                    RelatedOrganizationId = log.RelatedOrganizationId,
                    Operation = log.Operation,
                    OldData = log.OldData != null ? JsonSerializer.Deserialize<object>(log.OldData) : null,
                    NewData = log.NewData != null ? JsonSerializer.Deserialize<object>(log.NewData) : null,
                    ChangedAt = log.ChangedAt
                }).ToList();

                return dtos.AsReadOnly();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the GetOrganizationChangesQuery: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }
    }
}
