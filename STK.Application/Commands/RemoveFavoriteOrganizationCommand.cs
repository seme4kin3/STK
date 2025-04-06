using MediatR;

namespace STK.Application.Commands
{
    public class RemoveFavoriteOrganizationCommand : IRequest<Unit>
    {
        public Guid UserId { get; set; }
        public Guid OrganizationId { get; set; }
    }
}
