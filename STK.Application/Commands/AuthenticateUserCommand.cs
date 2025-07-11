using MediatR;
using STK.Application.DTOs.AuthDto;

namespace STK.Application.Commands
{
    public class AuthenticateUserCommand : IRequest<AuthTokenResponse>
    {
        public BaseUserDto AuthDto { get; }

        public AuthenticateUserCommand(BaseUserDto authDto)
        {
            AuthDto = authDto;
        }
    }
}
