using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetArbitrationCaseQueryHandler
       : IRequestHandler<GetArbitrationCaseQuery, IReadOnlyList<ArbitrationCaseDto>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetArbitrationCaseQueryHandler> _logger;

        public GetArbitrationCaseQueryHandler(
            DataContext dataContext,
            ILogger<GetArbitrationCaseQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ArbitrationCaseDto>> Handle(
            GetArbitrationCaseQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                if (query.OrganizationId == Guid.Empty)
                    throw new ArgumentException("OrganizationId cannot be empty.", nameof(query.OrganizationId));

                // Получаем название организации из своей таблицы организаций
                var organizationName = await _dataContext.Organizations
                    .AsNoTracking()
                    .Where(o => o.Id == query.OrganizationId)
                    .Select(o => o.Name) // подставь свой нужный атрибут
                    .FirstOrDefaultAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(organizationName))
                    throw new ArgumentException("Organization not found.", nameof(query.OrganizationId));

                var orgNameUpper = organizationName.ToUpper(); // для case-insensitive сравнения

                var arbitrationQuery = _dataContext.ArbitrationsCases
                    .AsNoTracking()
                    .Where(ac => ac.OrganizationId == query.OrganizationId);

                // Фильтрация по роли
                switch (query.Role)
                {
                    case ArbitrationPartyRole.Claimant:
                        arbitrationQuery = arbitrationQuery.Where(ac =>
                            ac.Claimant != null &&
                            ac.Claimant.ToUpper().Contains(orgNameUpper));
                        break;

                    case ArbitrationPartyRole.Respondent:
                        arbitrationQuery = arbitrationQuery.Where(ac =>
                            ac.Respondent != null &&
                            ac.Respondent.ToUpper().Contains(orgNameUpper));
                        break;

                    case ArbitrationPartyRole.Any:
                    default:
                        // без доп. фильтра
                        break;
                }

                var arbitrationCases = await arbitrationQuery
                    .Select(ac => new ArbitrationCaseDto
                    {
                        OrganizationId = ac.OrganizationId,
                        Id = ac.Id,
                        Instance = ac.Instance,
                        DateOfCreateCase = ac.DateOfCreateCase,
                        Claimant = ac.Claimant,
                        Respondent = ac.Respondent,
                        Judge = ac.Judge,
                        Url = ac.Url,
                        CaseNumber = ac.CaseNumber,
                    })
                    .OrderByDescending(ac => ac.DateOfCreateCase)
                    .ToListAsync(cancellationToken);

                return arbitrationCases;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while processing the GetArbitrationCaseQueryHandler: {Message}",
                    ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }
    }
}
