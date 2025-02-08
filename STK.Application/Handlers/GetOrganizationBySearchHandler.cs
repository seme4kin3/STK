using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetOrganizationBySearchHandler : IRequestHandler<GetOrganizationBySearchQuery, List<SearchOrganizationDTO>>
    {
        private readonly DataContext _dataContext;

        public GetOrganizationBySearchHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<SearchOrganizationDTO>> Handle(GetOrganizationBySearchQuery query, CancellationToken cancellationToken)
        {
            var allowedCodes = new List<string> { "30.20.9", "30.20.31", "52.21.1" };

            if (string.IsNullOrWhiteSpace(query.Search))
            {
                throw new ArgumentException("Search term cannot be null or whitespace.", nameof(query.Search));
            }

            var organizations = await _dataContext.Organizations
                .AsNoTracking() // Отключаем отслеживание изменений
                .Include(o => o.Requisites)
                .Include(o => o.Managements)
                .Include(o => o.EconomicActivities)
                .Where(o => o.Name.Contains(query.Search) ||
                            o.FullName.Contains(query.Search) ||
                            o.Requisites.INN.StartsWith(query.Search) ||
                            o.Requisites.OGRN.StartsWith(query.Search))
                .Where(o => o.EconomicActivities.Any(e => allowedCodes.Contains(e.OKVDNnumber))) // Фильтр организаций
                .Select(o => new SearchOrganizationDTO
                {
                    Id = o.Id,
                    Name = o.Name,
                    FullName = o.FullName,
                    Adress = o.Adress + o.IndexAdress,
                    Inn = o.Requisites.INN,
                    Ogrn = o.Requisites.OGRN,
                    Kpp = o.Requisites.KPP,
                    Managements = o.Managements.Select(m => new SearchManagementDTO
                    {
                        FullName = m.FullName,
                        Position = m.Position,
                    }).ToList(),
                    EconomicActivities = o.EconomicActivities
                        .Where(e => allowedCodes.Contains(e.OKVDNnumber)) // Фильтр видов деятельности
                        .ToList()
                }).ToListAsync(cancellationToken);

            return organizations;
        }
    }
}
