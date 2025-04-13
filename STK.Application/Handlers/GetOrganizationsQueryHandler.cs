using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetOrganizationsQueryHandler: IRequestHandler<GetOrganizationsQuery, IReadOnlyList<SearchOrganizationDTO>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetOrganizationsQueryHandler> _logger;
 
        public GetOrganizationsQueryHandler(DataContext dataContext, ILogger<GetOrganizationsQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<SearchOrganizationDTO>> Handle (GetOrganizationsQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var allowedCodes = new List<string> { "30.20.9", "30.20.31", "52.21.1" };

                var organizationStatuses = await _dataContext.AuditLog
                      .AsNoTracking()
                      .Where(log => new[] { "Organizations", "Requisites", "Managements" }.Contains(log.TableName))
                      .Where(log => new[] { "INSERT", "UPDATE" }.Contains(log.Operation))
                      .Where(log => log.ChangedAt >= DateTime.Today.AddMonths(-1) && log.ChangedAt <= DateTime.Now)
                      .Where(log => log.RelatedOrganizationId != null || log.TableName == "Organizations")
                      .GroupBy(log => log.TableName == "Organizations" ? log.RecordId : log.RelatedOrganizationId!.Value)
                      .Select(g => new
                      {
                          OrganizationId = g.Key,
                          Status = g.OrderByDescending(log => log.ChangedAt)
                                    .First()
                                    .Operation == "INSERT" ? "Новая" : "Изменённая"
                      })
                      .ToDictionaryAsync(
                          x => x.OrganizationId,
                          x => x.Status);

                var organizationIds = organizationStatuses.Keys;

                // Получаем организации
                var organizations = await _dataContext.Organizations
                    .AsNoTracking()
                    .Include(o => o.Requisites)
                    .Include(o => o.OrganizationsEconomicActivities)
                        .ThenInclude(oe => oe.EconomicActivities)
                    .Include(o => o.Managements)
                    .Include(o => o.FavoritedByUsers)
                    .Where(o => organizationIds.Contains(o.Id))
                    .Where(o => o.Requisites.INN != null && o.Name != null)
                    .OrderByDescending(o => o.Requisites.DateCreation)
                    .Take(50)
                    .ToListAsync();

                // Формируем DTO
                var dtos = organizations.Select(o => new SearchOrganizationDTO
                {
                    Id = o.Id,
                    Name = o.Name,
                    FullName = o.FullName,
                    Address = $"{o.Address} {o.IndexAddress}",
                    Inn = o.Requisites.INN,
                    Ogrn = o.Requisites.OGRN,
                    CreationDate = o.Requisites.DateCreation,
                    IsFavorite = o.FavoritedByUsers.Any(fu => fu.UserId == query.UserId),
                    StatusChange = organizationStatuses.ContainsKey(o.Id) ? organizationStatuses[o.Id] : "Неизвестно",
                    Managements = o.Managements.Select(m => new SearchManagementDTO
                    {
                        FullName = m.FullName,
                        Position = m.Position
                    }).ToList(),
                    SearchEconomicActivities = o.OrganizationsEconomicActivities
                        .Where(oea => oea.IsMain || allowedCodes.Contains(oea.EconomicActivities.OKVDNumber))
                        .Select(ea => new SearchEconomicActivityDto
                        {
                            OKVDNumber = ea.EconomicActivities.OKVDNumber,
                            Description = ea.EconomicActivities.Description
                        }).ToList()
                }).ToList();

                //var organizations = await _dataContext.Organizations
                //    .AsNoTracking()
                //    .Include(o => o.Requisites)
                //    .Include(o => o.OrganizationsEconomicActivities)
                //        .ThenInclude(oe => oe.EconomicActivities)
                //    .OrderByDescending(o => o.Requisites.DateCreation)
                //    .Where(o => o.Requisites.INN != null && o.Name != null)
                //    .Take(50)
                //    .Select(o => new SearchOrganizationDTO
                //    {
                //        Id = o.Id,
                //        Name = o.Name,
                //        FullName = o.FullName,
                //        Address = $"{o.Address} {o.IndexAddress}",
                //        Inn = o.Requisites.INN,
                //        Ogrn = o.Requisites.OGRN,
                //        CreationDate = o.Requisites.DateCreation,
                //        IsFavorite = o.FavoritedByUsers.Any(fu => fu.UserId == query.UserId),
                //        Managements = o.Managements.Select(m => new SearchManagementDTO
                //        {
                //            FullName = m.FullName,
                //            Position = m.Position,
                //        }).ToList(),
                //        StatusChange = "",
                //        SearchEconomicActivities = o.OrganizationsEconomicActivities
                //        .Where(oea => oea.IsMain || allowedCodes.Contains(oea.EconomicActivities.OKVDNumber))
                //        .Select(ea => new SearchEconomicActivityDto
                //        {
                //            OKVDNumber = ea.EconomicActivities.OKVDNumber,
                //            Description = ea.EconomicActivities.Description
                //        }).ToList(),
                //    }).ToListAsync(cancellationToken);

                return dtos;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }

        }
    }
}
