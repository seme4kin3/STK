using MediatR;
//using STK.Application.DTOs.AuthDto;
using STK.Application.DTOs.AuthDtoTest;

namespace STK.Application.Commands
{
    //public class AuthenticateUserCommand : IRequest<AuthTokenResponse>
    //{
    //    public UserDto AuthDto { get; }

    //    public AuthenticateUserCommand(UserDto authDto)
    //    {
    //        AuthDto = authDto;
    //    }
    //}

    public class AuthenticateUserCommand : IRequest<AuthTokenResponse>
    {
        public BaseUserDto AuthDto { get; }

        public AuthenticateUserCommand(BaseUserDto authDto)
        {
            AuthDto = authDto;
        }
    }
}
