using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetArbitrationCaseQueryHandler : IRequestHandler<GetArbitrationCaseQuery, IReadOnlyList<ArbitrationCaseDto>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetArbitrationCaseQueryHandler> _logger;

        public GetArbitrationCaseQueryHandler(DataContext dataContext, ILogger<GetArbitrationCaseQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ArbitrationCaseDto>> Handle(GetArbitrationCaseQuery query, CancellationToken cancellationToken)
        {
            try
            {
                if (query.OrganizationId == Guid.Empty)
                    throw new ArgumentException("OrganizationId cannot be empty.", nameof(query.OrganizationId));

                var arbitrationCase = await _dataContext.ArbitrationsCases
                    .AsNoTracking()
                    .Where(ac => ac.OrganizationId == query.OrganizationId)
                    .Select(ac => new  ArbitrationCaseDto
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
                    }).ToListAsync(cancellationToken);

                return arbitrationCase;
            }

            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while processing the GetArbitrationCaseQueryHandler: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }
    }
}
