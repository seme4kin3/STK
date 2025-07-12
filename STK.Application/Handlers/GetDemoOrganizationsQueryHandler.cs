using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Domain.Entities;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public record GetDemoOrganizationsQuery() : IRequest<IReadOnlyList<SearchOrganizationDTO>>;
    public class GetDemoOrganizationsQueryHandler : IRequestHandler<GetDemoOrganizationsQuery, IReadOnlyList<SearchOrganizationDTO>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetDemoOrganizationsQueryHandler> _logger;

        public GetDemoOrganizationsQueryHandler(DataContext dataContext, ILogger<GetDemoOrganizationsQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<SearchOrganizationDTO>> Handle (GetDemoOrganizationsQuery query, CancellationToken cancellationToken)
        {
            var organizations = await _dataContext.Organizations
                .AsSplitQuery()
                .AsNoTracking()
                .Include(o => o.Requisites)
                .Include(o => o.Managements)
                .Include(o => o.OrganizationsEconomicActivities)
                    .ThenInclude(oea => oea.EconomicActivities)
                .Where(o => o.Id == Guid.Parse("ca7d718a-3cc2-46f3-aa81-db26c923b9ef")
                    || o.Id == Guid.Parse("f6b96bb2-552b-4796-a6f0-a83e57f59bb0")
                    || o.Id == Guid.Parse("ba927673-e2c0-45cb-bfbd-5170c8577a34"))
                .ToListAsync(cancellationToken);

            return organizations.Select(o => new SearchOrganizationDTO
            {
                Id = o.Id,
                Name = o.Name,
                FullName = o.FullName,
                Address = $"{o.Address} {o.IndexAddress}".Trim(),
                Inn = o.Requisites?.INN ?? string.Empty,
                Ogrn = o.Requisites?.OGRN ?? string.Empty,
                Kpp = o.Requisites?.KPP ?? string.Empty,
                CreationDate = o.Requisites?.DateCreation,
                IsFavorite = false,
                StatusChange = string.Empty,
                Managements = o.Managements?
                    .Take(3)
                    .Select(m => new SearchManagementDTO
                    {
                        FullName = m.FullName ?? string.Empty,
                        Position = m.Position ?? string.Empty
                    })
                    .ToList() ?? new List<SearchManagementDTO>(),
                SearchEconomicActivities = o.OrganizationsEconomicActivities?
                    .Take(5)
                    .Select(oea => new SearchEconomicActivityDto
                    {
                        OKVDNumber = oea.EconomicActivities?.OKVDNumber ?? string.Empty,
                        Description = oea.EconomicActivities?.Description ?? string.Empty
                    })
                    .ToList() ?? new List<SearchEconomicActivityDto>(),
            }).ToList();
        }
    }
}
