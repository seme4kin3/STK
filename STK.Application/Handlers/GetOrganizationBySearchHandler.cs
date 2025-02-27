using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Pagination;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetOrganizationBySearchHandler : IRequestHandler<GetOrganizationBySearchQuery, PagedList<SearchOrganizationDTO>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetOrganizationBySearchHandler> _logger;
        public GetOrganizationBySearchHandler(DataContext dataContext, ILogger<GetOrganizationBySearchHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<PagedList<SearchOrganizationDTO>> Handle(GetOrganizationBySearchQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var allowedCodes = new List<string> { "30.20.9", "30.20.31", "52.21.1" };

                if (string.IsNullOrWhiteSpace(query.Search))
                {
                    throw new ArgumentException("Search term cannot be null or whitespace.", nameof(query.Search));
                }

                if (query.PageNumber < 1 || query.PageSize < 1)
                {
                    throw new ArgumentException("Page number and page size must be greater than 0.");
                }

                var organizationsQuery = _dataContext.Organizations
                    .AsNoTracking() // Отключаем отслеживание изменений
                    .Include(o => o.Requisites)
                    .Include(o => o.Managements)
                    .Include(o => o.EconomicActivities)
                    .Where(o => (o.Name.Contains(query.Search) ||
                                o.FullName.Contains(query.Search) ||
                                o.Requisites.INN.StartsWith(query.Search) ||
                                o.Requisites.OGRN.StartsWith(query.Search)) &&
                                o.EconomicActivities.Any(e => allowedCodes.Contains(e.OKVDnumber))); // Фильтр организаций

                // Получаем общее количество организаций
                var count = await organizationsQuery.CountAsync(cancellationToken);

                // Выполняем выбор элементов с применением проекции
                var items = await organizationsQuery
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(o => new SearchOrganizationDTO
                    {
                        Id = o.Id,
                        Name = o.Name,
                        FullName = o.FullName,
                        Adress = $"{o.Adress} {o.IndexAdress}",
                        Inn = o.Requisites.INN,
                        Ogrn = o.Requisites.OGRN,
                        Kpp = o.Requisites.KPP,
                        Managements = o.Managements
                            .Select(m => new SearchManagementDTO
                            {
                                FullName = m.FullName,
                                Position = m.Position,
                            }).ToList(),
                        EconomicActivities = o.EconomicActivities
                            .Where(e => allowedCodes.Contains(e.OKVDnumber))
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
