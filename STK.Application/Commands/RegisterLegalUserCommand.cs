using MediatR;
using STK.Application.DTOs.AuthDto;

namespace STK.Application.Commands
{
    public class RegisterLegalUserCommand : IRequest<Unit>
    {
        public LegalRegisterDto LegalRegisterDto { get; }

        public RegisterLegalUserCommand(LegalRegisterDto legalRegisterDto)
        {
            LegalRegisterDto = legalRegisterDto;
        }
    }
}
