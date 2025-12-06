using MediatR;
using STK.Application.DTOs;

namespace STK.Application.Queries
{
    public record GetArbitrationCaseQuery(
        Guid OrganizationId,
        ArbitrationPartyRole Role
    ) : IRequest<IReadOnlyList<ArbitrationCaseDto>>;
}
