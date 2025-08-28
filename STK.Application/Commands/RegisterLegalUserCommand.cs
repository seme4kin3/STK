using MediatR;
using STK.Application.DTOs.AuthDto;

namespace STK.Application.Commands
{
    public class RegisterLegalUserCommand : IRequest<Unit>
    {
        public LegalRegisterDto LegalRegisterDto { get; }
        public string IpAddress { get; }

        public RegisterLegalUserCommand(LegalRegisterDto legalRegisterDto, string ipAddress)
        {
            LegalRegisterDto = legalRegisterDto;
            IpAddress = ipAddress;
        }
    }
}
