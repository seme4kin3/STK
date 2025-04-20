using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Pagination;
using STK.Application.Queries;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class GetCertificateBySearchQueryHandler : IRequestHandler<GetCertificateBySearchQuery, PagedList<SearchCertificatesDto>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetCertificateBySearchQueryHandler> _logger;
        private static readonly Dictionary<string, string> statusCertificate = new Dictionary<string, string>
        {
            { "Actual", "Действующий" },
            { "Expired", "Истекший" },
        };

        public GetCertificateBySearchQueryHandler(DataContext dataContext, ILogger<GetCertificateBySearchQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<PagedList<SearchCertificatesDto>> Handle(GetCertificateBySearchQuery query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query.Search))
            {
                throw new ArgumentException("Search term cannot be null or whitespace.", nameof(query.Search));
            }

            if (query.PageNumber < 1 || query.PageSize < 1)
            {
                throw new ArgumentException("Page number and page size must be greater than 0.");
            }

            var countCertificate = await _dataContext.Certificates.CountAsync(cancellationToken);

            var certificateQuery = _dataContext.Certificates
                .AsNoTracking()
                .Where(c => c.CertificationObject.ToLower().Contains(query.Search.ToLower()));

            var items = await certificateQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new SearchCertificatesDto
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
                    Status = statusCertificate.GetValueOrDefault(c.Status, c.Status),
                    OrganizationId = c.OrganizationId
                }).ToListAsync(cancellationToken);

            return new PagedList<SearchCertificatesDto>(items, countCertificate, query.PageNumber, query.PageSize);
        }
    }
}
