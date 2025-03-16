
namespace STK.Application.DTOs.AuthDto
{
    public class AuthTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string UserName { get; set; }
        public Guid UserId { get; set; }
    }
}
