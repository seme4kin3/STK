using MediatR;
using STK.Application.DTOs;

namespace STK.Application.Queries
{
    public class GetArbitrationCaseQuery : IRequest<IReadOnlyList<ArbitrationCaseDto>>
    {
        public Guid OrganizationId { get; set; }
        public GetArbitrationCaseQuery(Guid organizationId) 
        {
            OrganizationId = organizationId;
        }
    }
}
