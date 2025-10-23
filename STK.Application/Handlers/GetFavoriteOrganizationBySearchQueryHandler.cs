using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Pagination;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetFavoriteOrganizationBySearchQueryHandler : IRequestHandler<GetFavoriteOrganizationBySearchQuery, PagedList<SearchOrganizationDTO>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetFavoriteOrganizationBySearchQueryHandler> _logger;
        private static readonly HashSet<string> AllowedOkvdCodes = new() { "30.20.9", "30.20.31", "52.21.1" };

        public GetFavoriteOrganizationBySearchQueryHandler(DataContext dataContext, ILogger<GetFavoriteOrganizationBySearchQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<PagedList<SearchOrganizationDTO>> Handle(GetFavoriteOrganizationBySearchQuery query, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query.Search))
                {
                    throw new ArgumentException("Search term cannot be null or whitespace.", nameof(query.Search));
                }

                if (query.PageNumber < 1 || query.PageSize < 1)
                {
                    throw new ArgumentException("Page number and page size must be greater than 0.");
                }

                var favoritesQuery = _dataContext.UsersFavoritesOrganizations
                    .AsNoTracking()
                    .Where(ufo => ufo.UserId == query.UserId &&
                        (
                            ufo.Organization.Name.ToLower().Contains(query.Search.ToLower()) ||
                            ufo.Organization.FullName.ToLower().Contains(query.Search.ToLower()) ||
                            ufo.Organization.Requisites.INN.ToLower().StartsWith(query.Search.ToLower()) ||
                            ufo.Organization.Requisites.OGRN.ToLower().StartsWith(query.Search.ToLower()) ||
                            ufo.Organization.OrganizationsEconomicActivities.Any(oe => oe.EconomicActivities.OKVDNumber.ToLower().StartsWith(query.Search.ToLower())) ||
                            ufo.Organization.OrganizationsEconomicActivities.Any(oe => oe.EconomicActivities.Description.ToLower().StartsWith(query.Search.ToLower()))
                        ))
                    .Include(ufo => ufo.Organization)
                        .ThenInclude(o => o.Requisites)
                    .Include(ufo => ufo.Organization)
                        .ThenInclude(o => o.Managements)
                    .Include(ufo => ufo.Organization)
                        .ThenInclude(o => o.OrganizationsEconomicActivities)
                            .ThenInclude(oea => oea.EconomicActivities);

                var count = await favoritesQuery.CountAsync(cancellationToken);

                var items = await favoritesQuery
                    .OrderByDescending(ufo => ufo.DateAddedOn)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(ufo => new SearchOrganizationDTO
                    {
                        Id = ufo.Organization.Id,
                        Name = ufo.Organization.Name,
                        FullName = ufo.Organization.FullName,
                        Address = $"{ufo.Organization.IndexAddress}",
                        AddressBool = ufo.Organization.Address,
                        Inn = ufo.Organization.Requisites.INN,
                        Ogrn = ufo.Organization.Requisites.OGRN,
                        Kpp = ufo.Organization.Requisites.KPP,
                        CreationDate = ufo.Organization.Requisites.DateCreation,
                        IsFavorite = true,
                        Managements = ufo.Organization.Managements
                            .Select(m => new SearchManagementDTO
                            {
                                FullName = m.FullName,
                                Position = m.Position,
                            })
                            .ToList(),
                        SearchEconomicActivities = ufo.Organization.OrganizationsEconomicActivities
                            .Where(oea => oea.IsMain == true || AllowedOkvdCodes.Contains(oea.EconomicActivities.OKVDNumber))
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
