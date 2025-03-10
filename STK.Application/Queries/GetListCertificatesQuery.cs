using MediatR;
using STK.Application.DTOs.SearchOrganizations;


namespace STK.Application.Queries
{
    public class GetListCertificatesQuery : IRequest<IReadOnlyList<ListCertificates>>
    {

    }
}
