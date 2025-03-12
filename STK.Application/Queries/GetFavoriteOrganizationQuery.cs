using MediatR;
using STK.Application.DTOs.SearchOrganizations;

namespace STK.Application.Queries
{
    public class GetFavoriteOrganizationQuery : IRequest<IReadOnlyList<SearchOrganizationDTO>>
    {
        public Guid UserId { get; set; }
    }
}
