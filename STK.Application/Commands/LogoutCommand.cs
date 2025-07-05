using MediatR;

namespace STK.Application.Commands
{
    public class LogoutCommand : IRequest<string>
    {
        public Guid UserId { get; set; }
    }
}
