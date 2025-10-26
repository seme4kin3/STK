using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Pagination;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    //public class GetOrganizationBySearchHandler : IRequestHandler<GetOrganizationBySearchQuery, PagedList<SearchOrganizationDTO>>
    //{
    //    private readonly DataContext _dataContext;
    //    private readonly ILogger<GetOrganizationBySearchHandler> _logger;
    //    private static readonly HashSet<string> AllowedOkvdCodes = new() { "30.20.9", "30.20.31", "52.21.1" };
    //    public GetOrganizationBySearchHandler(DataContext dataContext, ILogger<GetOrganizationBySearchHandler> logger)
    //    {
    //        _dataContext = dataContext;
    //        _logger = logger;
    //    }

    //    public async Task<PagedList<SearchOrganizationDTO>> Handle(GetOrganizationBySearchQuery query, CancellationToken cancellationToken)
    //    {
    //        try
    //        {
    //            if (string.IsNullOrWhiteSpace(query.Search))
    //            {
    //                throw new ArgumentException("Search term cannot be null or whitespace.", nameof(query.Search));
    //            }

    //            if (query.PageNumber < 1 || query.PageSize < 1)
    //            {
    //                throw new ArgumentException("Page number and page size must be greater than 0.");
    //            }

    //            var organizationsQuery = _dataContext.Organizations
    //                .AsNoTracking() // Отключаем отслеживание изменений для повышения производительности
    //                .Include(o => o.Requisites) 
    //                .Include(o => o.Managements) 
    //                .Include(o => o.OrganizationsEconomicActivities) 
    //                    .ThenInclude(oe => oe.EconomicActivities) 
    //                .Where(o =>
    //                    (o.Name.ToLower().Contains(query.Search.ToLower()) || // Поиск по названию организации
    //                     o.FullName.ToLower().Contains(query.Search.ToLower()) || // Поиск по полному названию организации
    //                     o.Requisites.INN.ToLower().StartsWith(query.Search.ToLower()) || // Поиск по ИНН
    //                     o.Requisites.OGRN.ToLower().StartsWith(query.Search.ToLower()) || // Поиск по ОГРН
    //                     o.OrganizationsEconomicActivities.Any(oe => oe.EconomicActivities.OKVDNumber.ToLower().StartsWith(query.Search.ToLower())) ||
    //                     o.OrganizationsEconomicActivities.Any(oe => oe.EconomicActivities.Description.ToLower().StartsWith(query.Search.ToLower()))
    //                     ) // Поиск по коду ОКВЭД
    //                );

    //            if (organizationsQuery == null)
    //            {
    //                return null;
    //            }

    //            // Получаем общее количество организаций
    //            var count = await organizationsQuery.CountAsync(cancellationToken);

    //            // Выполняем выбор элементов с применением маппинга
    //            var items = await organizationsQuery
    //                .Skip((query.PageNumber - 1) * query.PageSize)
    //                .Take(query.PageSize)
    //                .Select(o => new SearchOrganizationDTO
    //                {
    //                    Id = o.Id,
    //                    Name = o.Name,
    //                    FullName = o.FullName,
    //                    Address = $"{o.Address} {o.IndexAddress}", 
    //                    Inn = o.Requisites.INN,
    //                    Ogrn = o.Requisites.OGRN,
    //                    Kpp = o.Requisites.KPP,
    //                    CreationDate = o.Requisites.DateCreation,
    //                    IsFavorite = o.FavoritedByUsers.Any(fu => fu.UserId == query.UserId),
    //                    Managements = o.Managements
    //                        .Select(m => new SearchManagementDTO
    //                        {
    //                            FullName = m.FullName,
    //                            Position = m.Position,
    //                        })
    //                        .ToList(), 
    //                    SearchEconomicActivities = o.OrganizationsEconomicActivities
    //                        .Where(oea => oea.IsMain == true || AllowedOkvdCodes.Contains(oea.EconomicActivities.OKVDNumber)) 
    //                        .Select(e => new SearchEconomicActivityDto 
    //                        {
    //                            OKVDNumber = e.EconomicActivities.OKVDNumber,
    //                            Description = e.EconomicActivities.Description

    //                        })
    //                        .ToList()
    //                })
    //                .ToListAsync(cancellationToken);

    //            return new PagedList<SearchOrganizationDTO>(items, count, query.PageNumber, query.PageSize);
    //        }

    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
    //            throw new ApplicationException("An error occurred while processing the request.", ex);
    //        }
    //    }
    //}

    public class GetOrganizationBySearchHandler : IRequestHandler<GetOrganizationBySearchQuery, PagedList<SearchOrganizationDTO>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetOrganizationBySearchHandler> _logger;
        private static readonly HashSet<string> AllowedOkvdCodes = new() { "30.20.9", "30.20.31", "52.21.1" };

        public GetOrganizationBySearchHandler(DataContext dataContext, ILogger<GetOrganizationBySearchHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<PagedList<SearchOrganizationDTO>> Handle(GetOrganizationBySearchQuery query, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query.Search))
                    throw new ArgumentException("Search term cannot be null or whitespace.", nameof(query.Search));

                if (query.PageNumber < 1 || query.PageSize < 1)
                    throw new ArgumentException("Page number and page size must be greater than 0.");

                var search = query.Search.ToLower();

                // Базовый запрос + приватность:
                // - если организация не имеет записей в UserCreatedOrganizations -> она публична и видна всем
                // - если имеет -> видна только пользователю, который её создал
                var organizationsQuery = _dataContext.Organizations
                    .AsNoTracking()
                    .Include(o => o.Requisites)
                    .Include(o => o.Managements)
                    .Include(o => o.OrganizationsEconomicActivities).ThenInclude(oe => oe.EconomicActivities)
                    .Where(o =>
                        // Приватность
                        !_dataContext.UserCreatedOrganizations.Any(uc => uc.OrganizationId == o.Id) ||
                        _dataContext.UserCreatedOrganizations.Any(uc => uc.OrganizationId == o.Id && uc.UserId == query.UserId)
                    )
                    .Where(o =>
                        // Поиск
                        o.Name.ToLower().Contains(search) ||
                        o.FullName.ToLower().Contains(search) ||
                        o.Requisites.INN.ToLower().StartsWith(search) ||
                        o.Requisites.OGRN.ToLower().StartsWith(search) ||
                        o.OrganizationsEconomicActivities.Any(oe => oe.EconomicActivities.OKVDNumber.ToLower().StartsWith(search)) ||
                        o.OrganizationsEconomicActivities.Any(oe => oe.EconomicActivities.Description.ToLower().StartsWith(search))
                    );

                // Общее количество
                var count = await organizationsQuery.CountAsync(cancellationToken);

                // Выборка страницы
                var items = await organizationsQuery
                    .OrderByDescending(o => o.LastChangedAtDate.HasValue
                        ? (o.CreatedAtDate > o.LastChangedAtDate ? o.CreatedAtDate : o.LastChangedAtDate)
                        : o.CreatedAtDate)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(o => new SearchOrganizationDTO
                    {
                        Id = o.Id,
                        Name = o.Name,
                        FullName = o.FullName,
                        Address = $"{o.IndexAddress}",
                        AddressBool = o.Address,
                        Inn = o.Requisites.INN,
                        Ogrn = o.Requisites.OGRN,
                        Kpp = o.Requisites.KPP,
                        CreationDate = o.Requisites.DateCreation,
                        AddressAdded = o.AddressAdded,
                        IsFavorite = o.FavoritedByUsers.Any(fu => fu.UserId == query.UserId),
                        Managements = o.Managements
                            .Select(m => new SearchManagementDTO
                            {
                                FullName = m.FullName,
                                Position = m.Position,
                            })
                            .ToList(),
                        SearchEconomicActivities = o.OrganizationsEconomicActivities
                            .Where(oea => oea.IsMain || AllowedOkvdCodes.Contains(oea.EconomicActivities.OKVDNumber))
                            .Select(e => new SearchEconomicActivityDto
                            {
                                OKVDNumber = e.EconomicActivities.OKVDNumber,
                                Description = e.EconomicActivities.Description
                            })
                            .ToList()
                    })
                    .ToListAsync(cancellationToken);

                return new PagedList<SearchOrganizationDTO>(items, count, query.PageNumber, query.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }
    }
}
