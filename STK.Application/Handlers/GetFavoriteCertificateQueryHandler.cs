using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetFavoriteCertificateQueryHandler : IRequestHandler<GetFavoriteCertificateQuery,IReadOnlyList<SearchCertificatesDto>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetFavoriteCertificateQueryHandler> _logger;

        static Dictionary<string, string> statusCertificate = new Dictionary<string, string>
        {
            { "Actual", "Действующий" },
            { "Expired", "Истекший" },
            { "Paused", "Действие приостановалено" },
        };

        public GetFavoriteCertificateQueryHandler(DataContext dataContext, ILogger<GetFavoriteCertificateQueryHandler>logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<IReadOnlyList<SearchCertificatesDto>> Handle(GetFavoriteCertificateQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var certificates = await _dataContext.UsersFavoritesCertificates
                    .AsNoTracking()
                    .OrderByDescending(ufc => ufc.DateAddedOn)
                    .Include(ufc => ufc.Certificate)
                    .Where(ufc => ufc.UserId ==  query.UserId)
                    .ToListAsync(cancellationToken);

                if (certificates == null)
                {
                    return null;
                }

                var favoriteCertificate = certificates.Select(c => new SearchCertificatesDto
                {
                    Id = c.Certificate.Id,
                    Title = c.Certificate.Title,
                    Applicant = c.Certificate.Applicant,
                    Address = c.Certificate.Address,
                    Country = c.Certificate.Country,
                    CertificationObject = c.Certificate.CertificationObject,
                    DateOfIssueCertificate = c.Certificate.DateOfIssueCertificate,
                    DateOfCertificateExpiration = c.Certificate.DateOfCertificateExpiration,
                    CertificationType = c.Certificate.CertificationType,
                    Manufacturer = c.Certificate.Manufacturer,
                    ManufacturerCountry = c.Certificate.ManufacturerCountry,
                    Status = statusCertificate.GetValueOrDefault(c.Certificate.Status, c.Certificate.Status),
                    OrganizationId = c.Certificate.OrganizationId

                }).ToList();

 
                return favoriteCertificate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }
    }
}
