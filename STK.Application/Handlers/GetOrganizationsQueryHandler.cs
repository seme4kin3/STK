using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Domain.Entities;
using STK.Persistance;
using System.Linq.Expressions;

namespace STK.Application.Handlers
{
    public class GetOrganizationsQueryHandler : IRequestHandler<GetOrganizationsQuery, IReadOnlyList<SearchOrganizationDTO>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetOrganizationsQueryHandler> _logger;
        private static readonly HashSet<string> AllowedOkvdCodes = new() { "30.20.9", "30.20.31", "52.21.1" };

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

        //        // 1. Определяем базовый запрос организаций за последний месяц
        //        var organizationsQuery = _dataContext.Organizations
        //            .AsNoTracking()
        //            .Where(o => o.Name != null);

        //        // 2. Применяем фильтрацию по типу записей
        //        if (query.IsNew)
        //        {
        //            // Только новые организации
        //            organizationsQuery = organizationsQuery
        //                .Where(o => o.CreatedAtDate >= monthAgo && o.CreatedAtDate <= now);
        //        }
        //        else
        //        {
        //            // Только измененные организации
        //            organizationsQuery = organizationsQuery
        //                .Where(o => o.LastChangedAtDate.HasValue &&
        //                           o.LastChangedAtDate >= monthAgo &&
        //                           o.LastChangedAtDate <= now &&
        //                           (o.CreatedAtDate < monthAgo || !o.CreatedAtDate.HasValue || o.CreatedAtDate != o.LastChangedAtDate));
        //        }


        //        // 3. Получаем организации с основными данными
        //        var organizations = await organizationsQuery
        //            .Include(o => o.Requisites)
        //            .Include(o => o.Managements)
        //            .Include(o => o.FavoritedByUsers.Where(f => f.UserId == query.UserId))
        //            .AsSplitQuery()
        //            .OrderByDescending(o => o.LastChangedAtDate.HasValue
        //                ? (o.CreatedAtDate > o.LastChangedAtDate ? o.CreatedAtDate : o.LastChangedAtDate)
        //                : o.CreatedAtDate)
        //            .Take(50)
        //            .ToListAsync(cancellationToken);

        //        if (!organizations.Any())
        //            return new List<SearchOrganizationDTO>();

        //        // 4. Отдельно получаем экономические активности
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

        //        // 5. Собираем результат
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

        //            // Определяем статус организации
        //            var statusChange = DetermineOrganizationStatus(o, monthAgo);

        //            return new SearchOrganizationDTO
        //            {
        //                Id = o.Id,
        //                Name = o.Name,
        //                FullName = o.FullName,
        //                Address = $"{o.Address} {o.IndexAddress}".Trim(),
        //                Inn = requisites?.INN,
        //                Ogrn = requisites?.OGRN,
        //                Kpp = requisites?.KPP,
        //                CreationDate = requisites?.DateCreation,
        //                IsFavorite = o.FavoritedByUsers.Any(),
        //                StatusChange = statusChange,
        //                Managements = o.Managements?.Select(m => new SearchManagementDTO
        //                {
        //                    FullName = m.FullName,
        //                    Position = m.Position
        //                }).ToList() ?? new List<SearchManagementDTO>(),
        //                SearchEconomicActivities = activities
        //            };
        //        }).OrderByDescending(o => o.CreationDate).ToList();
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

                // Видимость:
                //  - Публичные (нет записей в UserCreatedOrganizations) — видны всем
                //  - Приватные (есть запись в UserCreatedOrganizations) — видны только своему создателю
                var organizationsQuery = _dataContext.Organizations
                    .AsNoTracking()
                    .Where(o => o.Name != null)
                    .Where(o =>
                        !_dataContext.UserCreatedOrganizations
                            .Any(uc => uc.OrganizationId == o.Id) // публичные
                        ||
                        _dataContext.UserCreatedOrganizations
                            .Any(uc => uc.OrganizationId == o.Id && uc.UserId == query.UserId) // приватные автора
                    );

                // Фильтрация по типу записей
                if (query.IsNew)
                {
                    organizationsQuery = organizationsQuery
                        .Where(o => o.CreatedAtDate >= monthAgo && o.CreatedAtDate <= now);
                }
                else
                {
                    organizationsQuery = organizationsQuery
                        .Where(o => o.LastChangedAtDate.HasValue &&
                                    o.LastChangedAtDate >= monthAgo &&
                                    o.LastChangedAtDate <= now &&
                                    (o.CreatedAtDate < monthAgo || !o.CreatedAtDate.HasValue || o.CreatedAtDate != o.LastChangedAtDate));
                }

                // Основные данные
                var organizations = await organizationsQuery
                    .Include(o => o.Requisites)
                    .Include(o => o.Managements)
                    .Include(o => o.FavoritedByUsers.Where(f => f.UserId == query.UserId))
                    .AsSplitQuery()
                    .OrderByDescending(o => o.LastChangedAtDate.HasValue
                        ? (o.CreatedAtDate > o.LastChangedAtDate ? o.CreatedAtDate : o.LastChangedAtDate)
                        : o.CreatedAtDate)
                    .Take(50)
                    .ToListAsync(cancellationToken);

                if (organizations.Count == 0)
                    return Array.Empty<SearchOrganizationDTO>();

                // Экономические активности
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

                // Результат
                return organizations
                    .Select(o =>
                    {
                        var requisites = o.Requisites;
                        var activities = economicActivities
                            .Where(ea => ea.OrganizationId == o.Id)
                            .Select(ea => new SearchEconomicActivityDto
                            {
                                OKVDNumber = ea.OKVDNumber,
                                Description = ea.Description
                            }).ToList();

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
                    })
                    .OrderByDescending(o => o.CreationDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }

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
    }


}
