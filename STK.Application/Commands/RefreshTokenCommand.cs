using MediatR;
//using STK.Application.DTOs.AuthDto;
using STK.Application.DTOs.AuthDtoTest;

namespace STK.Application.Commands
{
    //public class RefreshTokenCommand: IRequest<AuthTokenResponse>
    //{
    //    public RefreshTokenDto RefreshTokenRequest { get; set; }
    //}

    public class RefreshTokenCommand : IRequest<AuthTokenResponse>
    {
        public RefreshTokenRequestDto RefreshTokenRequest { get; set; }

        public RefreshTokenCommand(RefreshTokenRequestDto refreshTokenRequest)
        {
            RefreshTokenRequest = refreshTokenRequest;
        }
    }
}
