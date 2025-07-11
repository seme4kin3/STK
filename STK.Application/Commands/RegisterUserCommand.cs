using MediatR;
using STK.Application.DTOs.AuthDto;

namespace STK.Application.Commands
{
    public class RegisterUserCommand : IRequest<string>
    {
        public RegisterDto RegisterDto { get; set; }

        public RegisterUserCommand(RegisterDto registerDto)
        {
            RegisterDto = registerDto;
        }
    }
}

