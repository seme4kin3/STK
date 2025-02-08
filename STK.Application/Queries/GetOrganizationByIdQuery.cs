using MediatR;
using STK.Application.DTOs;

namespace STK.Application.Queries
{
    public class GetOrganizationByIdQuery : IRequest<OrganizationDto>
    {
        public Guid Id { get; set; }
        public GetOrganizationByIdQuery(Guid id) 
        {
            Id = id;
        }
    }
}
