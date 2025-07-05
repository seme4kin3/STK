using MediatR;
using STK.Application.DTOs.AuthDto;

namespace STK.Application.Commands
{
    public class AuthenticateUserCommand : IRequest<AuthTokenResponse>
    {
        public UserDto AuthDto { get; }

        public AuthenticateUserCommand(UserDto authDto)
        {
            AuthDto = authDto;
        }
    }
}
