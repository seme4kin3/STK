using MediatR;
using STK.Application.DTOs.AuthDto;

namespace STK.Application.Commands
{
    public class RegisterUserCommand : IRequest<string>
    {
        public RegisterDto RegisterDto { get; }
        public string IpAddress { get; }

        public RegisterUserCommand(RegisterDto registerDto, string ipAddress)
        {
            RegisterDto = registerDto;
            IpAddress = ipAddress;
        }
    }
}

