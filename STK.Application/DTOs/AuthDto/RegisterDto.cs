
namespace STK.Application.DTOs.AuthDto
{
    public class RegisterDto : BaseUserDto
    {
        //public RoleName Role { get; set; }
        public SubscriptionType SubscriptionType { get; set; }
        public bool IsAccepted { get; set; }
        public string? OfferVersion { get; set; }
        public string? OfferLink { get; set; }
        //[Required]
        //public decimal Amount { get; set; }
    }
}
