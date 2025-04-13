
using MediatR;
using STK.Application.DTOs.SearchOrganizations;

namespace STK.Application.Queries
{
    public class GetFavoriteCertificateQuery : IRequest<IReadOnlyList<SearchCertificatesDto>>
    {
        public Guid UserId { get; set; }
    }
}
