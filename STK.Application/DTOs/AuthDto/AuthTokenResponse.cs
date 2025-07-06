
namespace STK.Application.DTOs.AuthDto
{
    public class AuthTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string UserEmail { get; set; }
        public Guid UserId { get; set; }
        public string UserTypeSubscription {  get; set; }
        public int CountRequest { get; set; }
    }
}
