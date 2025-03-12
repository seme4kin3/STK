using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetFavoriteOrganizationQueryHandler : IRequestHandler<GetFavoriteOrganizationQuery, IReadOnlyList<SearchOrganizationDTO>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetFavoriteOrganizationQueryHandler> _logger;

        public GetFavoriteOrganizationQueryHandler(DataContext dataContext, ILogger<GetFavoriteOrganizationQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<SearchOrganizationDTO>> Handle(GetFavoriteOrganizationQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var organizations = await _dataContext.UsersFavoritesOrganizations
                    .AsNoTracking()
                    .OrderByDescending(ufo => ufo.DateAddedOn)
                    .Include(ufo => ufo.Organization)
                        .ThenInclude(o => o.Requisites)
                    .Include(ufo => ufo.Organization)
                        .ThenInclude(o => o.Managements)
                    .Include(ufo => ufo.Organization)
                        .ThenInclude(o => o.OrganizationsEconomicActivities)
                            .ThenInclude(oea => oea.EconomicActivities)
                    .Where(ufo => ufo.UserId == request.UserId)
                    .Select(ufo => new SearchOrganizationDTO
                    {
                        Id = ufo.Organization.Id,
                        Name = ufo.Organization.Name,
                        FullName = ufo.Organization.FullName,
                        Address = ufo.Organization.Address,
                        Inn = ufo.Organization.Requisites.INN,
                        Ogrn = ufo.Organization.Requisites.OGRN,
                        Kpp = ufo.Organization.Requisites.KPP,
                        Managements = ufo.Organization.Managements.Select(m => new SearchManagementDTO
                        {
                            FullName = m.FullName,
                            Position = m.Position
                        }).ToList(),
                        SearchEconomicActivities = ufo.Organization.OrganizationsEconomicActivities.Select(oea => new SearchEconomicActivityDto
                        {
                            OKVDNumber = oea.EconomicActivities.OKVDNumber,
                            Description = oea.EconomicActivities.Description
                        }).ToList()
                    })
                    .ToListAsync(cancellationToken);

                if(organizations == null)
                {
                    return null;
                }

                return organizations;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while fetching favorite organizations for user {UserId}", request.UserId);
                throw new ApplicationException("An error occurred while fetching favorite organizations.", ex);
            }
        }
    }
}
