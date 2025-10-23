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

            var searchTerm = query.Search.Trim();
            const double similarityThreshold = 0.2;
            var stemPattern = BuildStemPattern(searchTerm);

            var baseQuery = _dataContext.Certificates
                .Where(c => !string.IsNullOrEmpty(c.CertificationObject));

            //var certificateQuery = searchTerm.Length >= 3
            //    ? baseQuery
            //        .Select(c => new
            //        {
            //            Certificate = c,
            //            Similarity = EF.Functions.TrigramsSimilarity(c.CertificationObject!, searchTerm)
            //        })
            //        .Where(c => c.Similarity >= similarityThreshold
            //                    || EF.Functions.ILike(c.Certificate.CertificationObject!, $"%{searchTerm}%")
            //                    || (stemPattern != null && EF.Functions.ILike(c.Certificate.CertificationObject!, stemPattern)))
            //    : baseQuery
            //        .Where(c => EF.Functions.ILike(c.CertificationObject!, $"%{searchTerm}%")
            //                    || (stemPattern != null && EF.Functions.ILike(c.CertificationObject!, stemPattern)))
            //        .Select(c => new
            //        {
            //            Certificate = c,
            //            Similarity = 1.0
            //        });

            var certificateQuery = searchTerm.Length >= 3
            ? baseQuery
                .Select(c => new
                {
                    Certificate = c,
                    Similarity = EF.Functions.TrigramsSimilarity(c.CertificationObject!, searchTerm)
                })
                .Where(c => c.Similarity >= similarityThreshold
                            || EF.Functions.ILike(c.Certificate.CertificationObject!, $"%{searchTerm}%")
                            || EF.Functions.ILike(c.Certificate.Title!, $"%{searchTerm}%")
                            || (stemPattern != null && EF.Functions.ILike(c.Certificate.CertificationObject!, stemPattern))
                            || (stemPattern != null && EF.Functions.ILike(c.Certificate.Title!, stemPattern)))
            : baseQuery
                .Where(c => EF.Functions.ILike(c.CertificationObject!, $"%{searchTerm}%")
                            || EF.Functions.ILike(c.Title!, $"%{searchTerm}%")
                            || (stemPattern != null && EF.Functions.ILike(c.CertificationObject!, stemPattern))
                            || (stemPattern != null && EF.Functions.ILike(c.Title!, stemPattern)))
                .Select(c => new
                {
                    Certificate = c,
                    Similarity = 1.0
                });

            var countCertificate = await certificateQuery.CountAsync(cancellationToken);

            var items = await certificateQuery
                .OrderByDescending(c => c.Similarity)
                .ThenByDescending(c => c.Certificate.DateOfIssueCertificate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(c => new SearchCertificatesDto
                {
                    Id = c.Certificate.Id,
                    Title = c.Certificate.Title,
                    Applicant = c.Certificate.Applicant,
                    Address = c.Certificate.Address,
                    Country = c.Certificate.Country,
                    CertificationObject = c.Certificate.CertificationObject,
                    DateOfCertificateExpiration = c.Certificate.DateOfCertificateExpiration,
                    DateOfIssueCertificate = c.Certificate.DateOfIssueCertificate,
                    CertificationType = c.Certificate.CertificationType,
                    Status = statusCertificate.GetValueOrDefault(c.Certificate.Status, c.Certificate.Status),
                    OrganizationId = c.Certificate.OrganizationId
                }).ToListAsync(cancellationToken);


            return new PagedList<SearchCertificatesDto>(items, countCertificate, query.PageNumber, query.PageSize);
        }

        private static string? BuildStemPattern(string searchTerm)
        {
            if (searchTerm.Length < 2)
            {
                return null;
            }

            var stem = searchTerm.TrimEnd(RussianEndingCharacters);

            if (stem.Length < 2 || stem.Equals(searchTerm, System.StringComparison.Ordinal))
            {
                return null;
            }

            return $"%{stem}%";
        }

        private static readonly char[] RussianEndingCharacters =
        {
            'а', 'я', 'ы', 'и', 'й', 'е', 'ь', 'ю', 'о', 'ё', 'у',
            'А', 'Я', 'Ы', 'И', 'Й', 'Е', 'Ь', 'Ю', 'О', 'Ё', 'У'
        };
    }
}
