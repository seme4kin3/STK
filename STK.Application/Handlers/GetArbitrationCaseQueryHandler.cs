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
                        Role = ac.Role,
                        Type = ac.Type,
                        DateOfCreateCase = ac.DateOfCreateCase,
                        Court = ac.Court,
                        Magistrate = ac.Magistrate,
                        Url = ac.Url,
                        AmountOfClaim = ac.AmountOfClaim,
                        Status = ac.Status,
                        DateOfStatus = ac.DateOfStatus,
                        Authority = ac.Authority,
                        CourtJudgment = ac.CourtJudgment,
                        CaseRegisterNumber = ac.CaseRegisterNumber,
                        TypeOfJudicialAct = ac.TypeOfJudicialAct,
                        NameOfJudicialAct = ac.NameOfJudicialAct,
                        UrlOfJudicialAct = ac.UrlOfJudicialAct,
                        AdditionalInformation = ac.AdditionalInformation,
                        DateOfJudicialAct = ac.DateOfJudicialAct
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
