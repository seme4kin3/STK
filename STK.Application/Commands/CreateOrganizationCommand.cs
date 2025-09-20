using MediatR;
using STK.Application.DTOs;

namespace STK.Application.Commands
{
    public class CreateOrganizationCommand : IRequest<Unit>
    {
        public CreateOrganizationDto Organization { get; }
        public Guid UserId { get; }

        public CreateOrganizationCommand(CreateOrganizationDto organization, Guid userId)
        {
            Organization = organization ?? throw new ArgumentNullException(nameof(organization));
            UserId = userId;
        }
    }
}
