using MediatR;
using STK.Application.DTOs;

namespace STK.Application.Queries
{
    public class GetOrganizationByIdQuery : IRequest<OrganizationDto>
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public GetOrganizationByIdQuery(Guid id, Guid? userId) 
        {
            Id = id;
            UserId = userId;
        }
    }
}
