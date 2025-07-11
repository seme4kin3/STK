using MediatR;
using STK.Application.DTOs.AuthDto;


namespace STK.Application.Commands
{
    public class RefreshTokenCommand : IRequest<AuthTokenResponse>
    {
        public RefreshTokenRequestDto RefreshTokenRequest { get; set; }

        public RefreshTokenCommand(RefreshTokenRequestDto refreshTokenRequest)
        {
            RefreshTokenRequest = refreshTokenRequest;
        }
    }
}
