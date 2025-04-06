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
                var organizations = await _dataContext.Organizations
                    .AsNoTracking()
                    .Include(o => o.Requisites)
                    .Include(o => o.OrganizationsEconomicActivities)
                        .ThenInclude(oe => oe.EconomicActivities)
                    .OrderByDescending(o => o.Requisites.DateCreation)
                    .Where(o => o.Requisites.INN != null && o.Name != null)
                    .Take(50)
                    .Select(o => new SearchOrganizationDTO
                    {
                        Id = o.Id,
                        Name = o.Name,
                        FullName = o.FullName,
                        Address = $"{o.Address} {o.IndexAddress}",
                        Inn = o.Requisites.INN,
                        Ogrn = o.Requisites.OGRN,
                        CreationDate = o.Requisites.DateCreation,
                        IsFavorite = o.FavoritedByUsers.Any(fu => fu.UserId == query.UserId),
                        Managements = o.Managements.Select(m => new SearchManagementDTO
                        {
                            FullName = m.FullName,
                            Position = m.Position,
                        }).ToList(),
                        SearchEconomicActivities = o.OrganizationsEconomicActivities
                        .Where(oea => oea.IsMain || allowedCodes.Contains(oea.EconomicActivities.OKVDNumber))
                        .Select(ea => new SearchEconomicActivityDto
                        {
                            OKVDNumber = ea.EconomicActivities.OKVDNumber,
                            Description = ea.EconomicActivities.Description
                        }).ToList(),
                    }).ToListAsync(cancellationToken);

                return organizations;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }

        }
    }
}
