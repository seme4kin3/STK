using MediatR;
using STK.Application.DTOs.SearchOrganizations;


namespace STK.Application.Queries
{
    public class GetListCertificatesQuery : IRequest<IReadOnlyList<SearchCertificatesDto>>
    {
        public Guid UserId { get; set; }
        public GetListCertificatesQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}
