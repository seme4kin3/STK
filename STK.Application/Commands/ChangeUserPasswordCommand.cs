using MediatR;
using STK.Application.DTOs.AuthDto;

namespace STK.Application.Commands
{
    public record ChangeUserPasswordCommand (BaseUserDto BaseUserDto) : IRequest<bool>;
}
