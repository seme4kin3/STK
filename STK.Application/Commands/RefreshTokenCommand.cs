using MediatR;
using STK.Application.DTOs.AuthDto;


namespace STK.Application.Commands
{
    public class RefreshTokenCommand : IRequest<AuthTokenResponse>
    {
        public string RefreshToken { get; set; }

        public RefreshTokenCommand(string refreshTokenRequest)
        {
            this.RefreshToken = refreshTokenRequest;
        }
    }
}
