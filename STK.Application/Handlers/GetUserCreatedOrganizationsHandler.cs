using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public sealed class GetUserCreatedOrganizationsQuery : IRequest<UserCreatedOrganizationsResultDto>
    {
        public Guid UserId { get; init; }
    }
    public class GetUserCreatedOrganizationsHandler : IRequestHandler<GetUserCreatedOrganizationsQuery, UserCreatedOrganizationsResultDto>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetUserCreatedOrganizationsHandler> _logger;

        public GetUserCreatedOrganizationsHandler(
            DataContext dataContext,
            ILogger<GetUserCreatedOrganizationsHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<UserCreatedOrganizationsResultDto> Handle(GetUserCreatedOrganizationsQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // 1) Все организации, созданные пользователем
                var orgIds = await _dataContext.UserCreatedOrganizations
                    .AsNoTracking()
                    .Where(uc => uc.UserId == query.UserId)
                    .Select(uc => uc.OrganizationId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                if (orgIds.Count == 0)
                {
                    return new UserCreatedOrganizationsResultDto
                    {
                        Organizations = new List<SearchOrganizationDTO>(),
                        NotLoadedInns = new List<string>()
                    };
                }

                // 2) Загружаем организации и маппим в SearchOrganizationDTO
                var organizations = await _dataContext.Organizations
                    .AsNoTracking()
                    .Where(o => orgIds.Contains(o.Id))
                    .Include(o => o.Requisites)
                    .Include(o => o.Managements)
                    .Include(o => o.FavoritedByUsers.Where(f => f.UserId == query.UserId))
                    .OrderByDescending(o => o.LastChangedAtDate.HasValue
                        ? (o.CreatedAtDate > o.LastChangedAtDate ? o.CreatedAtDate : o.LastChangedAtDate)
                        : o.CreatedAtDate)
                    .Select(o => new SearchOrganizationDTO
                    {
                        Id = o.Id,
                        Name = o.Name,
                        FullName = o.FullName,
                        Address = $"{o.Address} {o.IndexAddress}",
                        Inn = o.Requisites.INN,
                        Ogrn = o.Requisites.OGRN,
                        Kpp = o.Requisites.KPP,
                        CreationDate = o.Requisites.DateCreation,
                        Managements = o.Managements.Select(m => new SearchManagementDTO
                        {
                            FullName = m.FullName,
                            Position = m.Position
                        }).ToList(),
                        SearchEconomicActivities = o.OrganizationsEconomicActivities.Select(oea => new SearchEconomicActivityDto
                        {
                            OKVDNumber = oea.EconomicActivities.OKVDNumber,
                            Description = oea.EconomicActivities.Description
                        }).ToList()
                    })
                    .ToListAsync(cancellationToken);

                // 3) ИНН незагруженных организаций (IsLoad == false) из OrganizationDownload
                var notLoadedInns = await _dataContext.OrganizationDownload
                    .AsNoTracking()
                    .Where(od => orgIds.Contains(od.Id) && od.IsLoad == false && !string.IsNullOrWhiteSpace(od.Inn))
                    .Select(od => od.Inn!)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                return new UserCreatedOrganizationsResultDto
                {
                    Organizations = organizations,
                    NotLoadedInns = notLoadedInns
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user-created organizations for {UserId}", query.UserId);
                throw new ApplicationException("An error occurred while fetching user-created organizations.", ex);
            }
        }
    }
}
