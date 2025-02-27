using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetOrganizationsQueryHandler: IRequestHandler<GetOrganizationsQuery, List<SearchOrganizationDTO>>
    {
        private readonly DataContext _dataContext;
        public GetOrganizationsQueryHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<SearchOrganizationDTO>> Handle (GetOrganizationsQuery query, CancellationToken cancellationToken)
        {
            var organizations = await _dataContext.Organizations
                .AsNoTracking()
                .Include(o => o.Requisites)
                .Include(o => o.EconomicActivities)
                .OrderByDescending(o => o.Requisites.DateCreation)
                .Take(50)
                .Select(o => new SearchOrganizationDTO
                {
                    Id = o.Id,
                    Name = o.Name,
                    FullName = o.FullName,
                    Adress = o.Adress + o.IndexAdress,
                    Inn = o.Requisites.INN,
                    Ogrn = o.Requisites.OGRN,
                    CreationDate = o.Requisites.DateCreation,
                    Managements = o.Managements.Select(m => new SearchManagementDTO
                    {
                        FullName = m.FullName,
                        Position = m.Position,
                    }).ToList(),
                    EconomicActivities = o.EconomicActivities.ToList(),
                }).ToListAsync(cancellationToken);

            return organizations;
        }
    }
}
