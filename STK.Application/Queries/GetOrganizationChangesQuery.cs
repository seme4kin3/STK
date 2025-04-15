using MediatR;
using STK.Application.DTOs;

namespace STK.Application.Queries
{
    public class GetOrganizationChangesQuery : IRequest<IReadOnlyList<AuditLogDto>>
    {
        public Guid OrganizationId { get; set; }
        public GetOrganizationChangesQuery(Guid id)
        {
            OrganizationId = id;
        }
    }
}
