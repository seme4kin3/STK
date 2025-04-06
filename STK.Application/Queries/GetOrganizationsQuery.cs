using MediatR;
using STK.Application.DTOs.SearchOrganizations;

namespace STK.Application.Queries
{
    public class GetOrganizationsQuery : IRequest<IReadOnlyList<SearchOrganizationDTO>>
    {
        public Guid UserId { get; set; }
        public GetOrganizationsQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}
