using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetListCertificatesHandler : IRequestHandler<GetListCertificatesQuery, IReadOnlyList<SearchCertificatesDto>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetListCertificatesHandler> _logger;
        public GetListCertificatesHandler(DataContext dataContext, ILogger<GetListCertificatesHandler> logger) 
        {
            _dataContext = dataContext;
            _logger = logger;
        }
        public async Task<IReadOnlyList<SearchCertificatesDto>> Handle(GetListCertificatesQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var certificates = await _dataContext.Certificates
                    .AsNoTracking()
                    .OrderByDescending(c => c.DateOfIssueCertificate)
                    .Take(50)
                    .Select(c => new SearchCertificatesDto
                    {
                        Title = c.Title,
                        Applicant = c.Applicant,
                        CertificationObject = c.CertificationObject,
                        Address = c.Address,
                        Country = c.Country,
                        DateOfIssueCertificate = c.DateOfIssueCertificate,
                        DateOfCertificateExpiration = c.DateOfCertificateExpiration,
                        CertificationType = c.CertificationType,
                        Status = c.Status,
                        OrganizationId = c.OrganizationId,
                    }).ToListAsync(cancellationToken);

                if (certificates == null)
                {
                    return null;
                }

                return certificates;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }
    }
}
