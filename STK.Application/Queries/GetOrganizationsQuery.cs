using MediatR;
using STK.Application.DTOs.SearchOrganizations;

namespace STK.Application.Queries
{
    public class GetOrganizationsQuery : IRequest<IReadOnlyList<SearchOrganizationDTO>>
    {
        public Guid UserId { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsChange {  get; set; }
        public GetOrganizationsQuery(Guid userId, bool? isNew, bool? isChange)
        {
            UserId = userId;
            IsNew = isNew;
            IsChange = isChange;
        }
    }
}
