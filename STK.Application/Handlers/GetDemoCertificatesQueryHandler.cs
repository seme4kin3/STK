using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs;
using STK.Application.DTOs.SearchOrganizations;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public record GetDemoCertificatesQuery() : IRequest<IReadOnlyList<SearchCertificatesDto>>;
    public class GetDemoCertificatesQueryHandler : IRequestHandler<GetDemoCertificatesQuery, IReadOnlyList<SearchCertificatesDto>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetDemoCertificatesQueryHandler> _logger;
        private static readonly Dictionary<string, string> statusCertificate = new Dictionary<string, string>
        {
            { "Actual", "Действующий" },
            { "Expired", "Истекший" },
        };

        public GetDemoCertificatesQueryHandler(DataContext dataContext, ILogger<GetDemoCertificatesQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<SearchCertificatesDto>> Handle(GetDemoCertificatesQuery query, CancellationToken cancellationToken)
        {
            var certificates = await _dataContext.Certificates
                .Where(c => c.OrganizationId == Guid.Parse("f6b96bb2-552b-4796-a6f0-a83e57f59bb0"))
                .Take(5)
                .ToListAsync();

            return certificates.Select(c => new SearchCertificatesDto
            {
                Id = c.Id,
                Title = c.Title,
                Applicant = c.Applicant,
                Address = c.Address,
                Country = c.Country,
                CertificationObject = c.CertificationObject,
                DateOfCertificateExpiration = c.DateOfCertificateExpiration,
                DateOfIssueCertificate = c.DateOfIssueCertificate,
                CertificationType = c.CertificationType,
                Manufacturer = c.Manufacturer,
                ManufacturerCountry = c.ManufacturerCountry,
                Status = statusCertificate.GetValueOrDefault(c.Status, c.Status),
                OrganizationId = c.OrganizationId
            }).ToList();
        }

    }
}
