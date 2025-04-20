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

        private static readonly Dictionary<string, string> statusCertificate = new Dictionary<string, string>
        {
            { "Actual", "Действующий" },
            { "Expired", "Истекший" },
            { "Paused", "Действие приостановалено" },
        };
        public GetListCertificatesHandler(DataContext dataContext, ILogger<GetListCertificatesHandler> logger) 
        {
            _dataContext = dataContext;
            _logger = logger;
        }
        public async Task<IReadOnlyList<SearchCertificatesDto>> Handle(GetListCertificatesQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var monthAgo = DateTime.UtcNow.Date.AddMonths(-1);
                var now = DateTime.UtcNow;

                var certificateStatuses = await _dataContext.AuditLog
                    .AsNoTracking()
                    .Where(log => log.TableName == "Certificates")
                    .Where(log => new[] { "INSERT", "UPDATE" }.Contains(log.Operation))
                    .Where(log => log.ChangedAt >= monthAgo && log.ChangedAt <= now)
                    .GroupBy(log => log.RecordId)
                    .Select(g => new
                    {
                        CertificateId = g.Key,
                        Status = g.OrderByDescending(log => log.ChangedAt)
                                  .First()
                                  .Operation == "INSERT" ? "Новая" : "Изменённая"
                    })
                    .ToDictionaryAsync(x => x.CertificateId,x => x.Status, cancellationToken);

                var certificateIds = certificateStatuses.Keys;

                // Получаем сертификаты
                var certificates = await _dataContext.Certificates
                    .AsNoTracking()
                    .Include(c => c.FavoritedByUsers)
                    .Where(c => certificateIds.Contains(c.Id))
                    .OrderByDescending(c => c.DateOfIssueCertificate)
                    .Take(50)
                    .Select(c => new SearchCertificatesDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Applicant = c.Applicant,
                        CertificationObject = c.CertificationObject,
                        Address = c.Address,
                        Country = c.Country,
                        DateOfIssueCertificate = c.DateOfIssueCertificate,
                        DateOfCertificateExpiration = c.DateOfCertificateExpiration,
                        CertificationType = c.CertificationType,
                        Status = statusCertificate.GetValueOrDefault(c.Status, c.Status),
                        IsFavorite = c.FavoritedByUsers.Any(fu => fu.UserId == query.UserId),
                        OrganizationId = c.OrganizationId,
                        StatusChange = certificateStatuses.ContainsKey(c.Id) ? certificateStatuses[c.Id] : "Неизвестно"
                    })
                    .ToListAsync(cancellationToken);
                //var certificates = await _dataContext.Certificates
                //    .AsNoTracking()
                //    .OrderByDescending(c => c.DateOfIssueCertificate)
                //    .Take(50)
                //    .Select(c => new SearchCertificatesDto
                //    {
                //        Id = c.Id,
                //        Title = c.Title,
                //        Applicant = c.Applicant,
                //        CertificationObject = c.CertificationObject,
                //        Address = c.Address,
                //        Country = c.Country,
                //        DateOfIssueCertificate = c.DateOfIssueCertificate,
                //        DateOfCertificateExpiration = c.DateOfCertificateExpiration,
                //        CertificationType = c.CertificationType,
                //        Status = statusCertificate.GetValueOrDefault(c.Status, c.Status),
                //        OrganizationId = c.OrganizationId,
                //    }).ToListAsync(cancellationToken);

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
