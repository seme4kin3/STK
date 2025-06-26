using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Domain.Entities;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetOrganizationsQueryHandler: IRequestHandler<GetOrganizationsQuery, IReadOnlyList<SearchOrganizationDTO>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetOrganizationsQueryHandler> _logger;
        private static readonly HashSet<string> AllowedOkvdCodes = new() { "30.20.9", "30.20.31", "52.21.1" };
        private static readonly HashSet<string> RelevantTables = new() { "Organizations", "Requisites", "Managements" };
        private static readonly string[] RelevantOperations = { "INSERT", "UPDATE" };

        public GetOrganizationsQueryHandler(DataContext dataContext, ILogger<GetOrganizationsQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        //public async Task<IReadOnlyList<SearchOrganizationDTO>> Handle(GetOrganizationsQuery query, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var monthAgo = DateTime.UtcNow.Date.AddMonths(-1);
        //        var now = DateTime.UtcNow;

        //        var auditLogs = await _dataContext.AuditLog
        //            .AsNoTracking()
        //            .Where(log => log.ChangedAt >= monthAgo && log.ChangedAt <= now)
        //            .Where(log => (log.TableName == "Organizations" && RelevantOperations.Contains(log.Operation)) ||
        //                         (RelevantTables.Contains(log.TableName) &&
        //                          RelevantOperations.Contains(log.Operation) &&
        //                          log.RelatedOrganizationId != null))
        //            .Select(log => new
        //            {
        //                log.RecordId,
        //                log.RelatedOrganizationId,
        //                log.TableName,
        //                log.Operation,
        //                log.ChangedAt
        //            })
        //            .ToListAsync(cancellationToken);

        //        var statusesDict = auditLogs
        //            .GroupBy(log => log.TableName == "Organizations" ? log.RecordId : log.RelatedOrganizationId!.Value)
        //            .ToDictionary(
        //                g => g.Key,
        //                g => g.OrderByDescending(log => log.ChangedAt)
        //                      .First()
        //                      .Operation == "INSERT" ? "Новая" : "Изменённая"
        //            );

        //        if (query.IsNew == true || query.IsChange == true)
        //        {
        //            statusesDict = statusesDict
        //                .Where(kvp => (query.IsNew == true && kvp.Value == "Новая") ||
        //                             (query.IsChange == true && kvp.Value == "Изменённая"))
        //                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        //        }

        //        if (statusesDict.Count == 0)
        //            return new List<SearchOrganizationDTO>();

        //        var organizationIds = statusesDict.Keys.ToList();

        //        // 2. Получаем организации с основными данными
        //        var organizations = await _dataContext.Organizations
        //            .AsNoTracking()
        //            .Where(o => organizationIds.Contains(o.Id) && o.Name != null)
        //            .Include(o => o.Requisites)
        //            .Include(o => o.Managements)
        //            .Include(o => o.FavoritedByUsers.Where(f => f.UserId == query.UserId))
        //            .AsSplitQuery()
        //            .OrderByDescending(o => o.Requisites.DateCreation)
        //            .Take(50)
        //            .ToListAsync(cancellationToken);

        //        // 3. Отдельно получаем экономические активности
        //        var orgIds = organizations.Select(o => o.Id).ToList();
        //        var economicActivities = await _dataContext.OrganizationsEconomicActivities
        //            .AsNoTracking()
        //            .Where(oea => orgIds.Contains(oea.OrganizationId) &&
        //                         (oea.IsMain || AllowedOkvdCodes.Contains(oea.EconomicActivities.OKVDNumber)))
        //            .Select(oea => new
        //            {
        //                oea.OrganizationId,
        //                oea.EconomicActivities.OKVDNumber,
        //                oea.EconomicActivities.Description
        //            })
        //            .ToListAsync(cancellationToken);

        //        // 4. Собираем результат
        //        return organizations.Select(o =>
        //        {
        //            var requisites = o.Requisites;
        //            var activities = economicActivities
        //                .Where(ea => ea.OrganizationId == o.Id)
        //                .Select(ea => new SearchEconomicActivityDto
        //                {
        //                    OKVDNumber = ea.OKVDNumber,
        //                    Description = ea.Description
        //                }).ToList();

        //            return new SearchOrganizationDTO
        //            {
        //                Id = o.Id,
        //                Name = o.Name,
        //                FullName = o.FullName,
        //                Address = $"{o.Address} {o.IndexAddress}".Trim(),
        //                Inn = requisites.INN,
        //                Ogrn = requisites.OGRN,
        //                Kpp = requisites.KPP,
        //                CreationDate = requisites.DateCreation,
        //                IsFavorite = o.FavoritedByUsers.Any(),
        //                //StatusChange = statusesDict.GetValueOrDefault(o.Id, "Неизвестно"),
        //                Managements = o.Managements.Select(m => new SearchManagementDTO
        //                {
        //                    FullName = m.FullName,
        //                    Position = m.Position
        //                }).ToList(),
        //                SearchEconomicActivities = activities
        //            };
        //        }).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
        //        throw new ApplicationException("An error occurred while processing the request.", ex);
        //    }

        //}
        public async Task<IReadOnlyList<SearchOrganizationDTO>> Handle(GetOrganizationsQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var monthAgo = DateTime.UtcNow.Date.AddMonths(-1);
                var now = DateTime.UtcNow;

                // 1. Определяем базовый запрос организаций за последний месяц
                var organizationsQuery = _dataContext.Organizations
                    .AsNoTracking()
                    .Where(o => o.Name != null);

                // 2. Применяем фильтрацию по типу записей
                if (query.IsNew == true && query.IsChange != true)
                {
                    // Только новые организации
                    organizationsQuery = organizationsQuery
                        .Where(o => o.CreatedAtDate >= monthAgo && o.CreatedAtDate <= now);
                }
                else if (query.IsChange == true && query.IsNew != true)
                {
                    // Только измененные организации
                    organizationsQuery = organizationsQuery
                        .Where(o => o.LastChangedAtDate.HasValue &&
                                   o.LastChangedAtDate >= monthAgo &&
                                   o.LastChangedAtDate <= now &&
                                   (o.CreatedAtDate < monthAgo || !o.CreatedAtDate.HasValue || o.CreatedAtDate != o.LastChangedAtDate));
                }
                else if (query.IsNew == true && query.IsChange == true)
                {
                    // И новые, и измененные
                    organizationsQuery = organizationsQuery
                        .Where(o => (o.CreatedAtDate >= monthAgo && o.CreatedAtDate <= now) ||
                                   (o.LastChangedAtDate.HasValue && o.LastChangedAtDate >= monthAgo && o.LastChangedAtDate <= now));
                }
                else
                {
                    // По умолчанию - все последние записи (новые и измененные) за месяц
                    organizationsQuery = organizationsQuery
                        .Where(o => (o.CreatedAtDate >= monthAgo && o.CreatedAtDate <= now) ||
                                   (o.LastChangedAtDate.HasValue && o.LastChangedAtDate >= monthAgo && o.LastChangedAtDate <= now));
                }

                // 3. Получаем организации с основными данными
                var organizations = await organizationsQuery
                    .Include(o => o.Requisites)
                    .Include(o => o.Managements)
                    .Include(o => o.FavoritedByUsers.Where(f => f.UserId == query.UserId))
                    .AsSplitQuery()
                    .OrderByDescending(o => GetLatestDate(o.CreatedAtDate, o.LastChangedAtDate))
                    .Take(50)
                    .ToListAsync(cancellationToken);

                if (!organizations.Any())
                    return new List<SearchOrganizationDTO>();

                // 4. Отдельно получаем экономические активности
                var orgIds = organizations.Select(o => o.Id).ToList();
                var economicActivities = await _dataContext.OrganizationsEconomicActivities
                    .AsNoTracking()
                    .Where(oea => orgIds.Contains(oea.OrganizationId) &&
                                 (oea.IsMain || AllowedOkvdCodes.Contains(oea.EconomicActivities.OKVDNumber)))
                    .Select(oea => new
                    {
                        oea.OrganizationId,
                        oea.EconomicActivities.OKVDNumber,
                        oea.EconomicActivities.Description
                    })
                    .ToListAsync(cancellationToken);

                // 5. Собираем результат
                return organizations.Select(o =>
                {
                    var requisites = o.Requisites;
                    var activities = economicActivities
                        .Where(ea => ea.OrganizationId == o.Id)
                        .Select(ea => new SearchEconomicActivityDto
                        {
                            OKVDNumber = ea.OKVDNumber,
                            Description = ea.Description
                        }).ToList();

                    // Определяем статус организации
                    var statusChange = DetermineOrganizationStatus(o, monthAgo);

                    return new SearchOrganizationDTO
                    {
                        Id = o.Id,
                        Name = o.Name,
                        FullName = o.FullName,
                        Address = $"{o.Address} {o.IndexAddress}".Trim(),
                        Inn = requisites?.INN,
                        Ogrn = requisites?.OGRN,
                        Kpp = requisites?.KPP,
                        CreationDate = requisites?.DateCreation,
                        IsFavorite = o.FavoritedByUsers.Any(),
                        StatusChange = statusChange,
                        Managements = o.Managements?.Select(m => new SearchManagementDTO
                        {
                            FullName = m.FullName,
                            Position = m.Position
                        }).ToList() ?? new List<SearchManagementDTO>(),
                        SearchEconomicActivities = activities
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }

        /// <summary>
        /// Определяет статус организации на основе дат создания и изменения
        /// </summary>
        private string DetermineOrganizationStatus(Organization organization, DateTime monthAgo)
        {
            var createdDate = organization.CreatedAtDate;
            var changedDate = organization.LastChangedAtDate;

            // Если организация создана в последний месяц
            if (createdDate.HasValue && createdDate >= monthAgo)
            {
                return "Новая";
            }

            // Если организация изменена в последний месяц (и создана раньше месяца назад или дата создания неизвестна)
            if (changedDate.HasValue && changedDate >= monthAgo)
            {
                // Дополнительная проверка: если дата изменения отличается от даты создания
                if (!createdDate.HasValue || createdDate < monthAgo || createdDate != changedDate)
                {
                    return "Изменённая";
                }
            }

            return "Неопределено";
        }

        /// <summary>
        /// Возвращает более позднюю из двух дат для сортировки
        /// </summary>
        private DateTime GetLatestDate(DateTime? createdDate, DateTime? changedDate)
        {
            if (!createdDate.HasValue && !changedDate.HasValue)
                return DateTime.MinValue;

            if (!createdDate.HasValue)
                return changedDate.Value;

            if (!changedDate.HasValue)
                return createdDate.Value;

            return createdDate > changedDate ? createdDate.Value : changedDate.Value;
        }
    }
}
