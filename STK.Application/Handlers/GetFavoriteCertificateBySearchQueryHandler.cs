using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.SearchOrganizations;
using STK.Application.Pagination;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetFavoriteCertificateBySearchQueryHandler : IRequestHandler<GetFavoriteCertificateBySearchQuery, PagedList<SearchCertificatesDto>>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<GetFavoriteCertificateBySearchQueryHandler> _logger;
        private static readonly Dictionary<string, string> statusCertificate = new Dictionary<string, string>
        {
            { "Actual", "Действующий" },
            { "Expired", "Истекший" },
            { "Paused", "Действие приостановалено" }
        };

        public GetFavoriteCertificateBySearchQueryHandler(DataContext dataContext, ILogger<GetFavoriteCertificateBySearchQueryHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<PagedList<SearchCertificatesDto>> Handle(GetFavoriteCertificateBySearchQuery query, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query.Search))
                {
                    throw new ArgumentException("Search term cannot be null or whitespace.", nameof(query.Search));
                }

                if (query.PageNumber < 1 || query.PageSize < 1)
                {
                    throw new ArgumentException("Page number and page size must be greater than 0.");
                }

                var favoritesQuery = _dataContext.UsersFavoritesCertificates
                    .AsNoTracking()
                    .Where(ufc => ufc.UserId == query.UserId &&
                        ufc.Certificate.CertificationObject.ToLower().Contains(query.Search.ToLower()))
                    .Include(ufc => ufc.Certificate);

                var count = await favoritesQuery.CountAsync(cancellationToken);

                var items = await favoritesQuery
                    .OrderByDescending(ufc => ufc.DateAddedOn)
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(ufc => new SearchCertificatesDto
                    {
                        Id = ufc.Certificate.Id,
                        Title = ufc.Certificate.Title,
                        Applicant = ufc.Certificate.Applicant,
                        Address = ufc.Certificate.Address,
                        Country = ufc.Certificate.Country,
                        CertificationObject = ufc.Certificate.CertificationObject,
                        DateOfCertificateExpiration = ufc.Certificate.DateOfCertificateExpiration,
                        DateOfIssueCertificate = ufc.Certificate.DateOfIssueCertificate,
                        CertificationType = ufc.Certificate.CertificationType,
                        Status = statusCertificate.GetValueOrDefault(ufc.Certificate.Status, ufc.Certificate.Status),
                        OrganizationId = ufc.Certificate.OrganizationId,
                        IsFavorite = true
                    })
                    .ToListAsync(cancellationToken);

                return new PagedList<SearchCertificatesDto>(items, count, query.PageNumber, query.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request: {Message}", ex.Message);
                throw new ApplicationException("An error occurred while processing the request.", ex);
            }
        }
    }
}