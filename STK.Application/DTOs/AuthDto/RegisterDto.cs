
namespace STK.Application.DTOs.AuthDto
{
    public class RegisterDto : BaseUserDto
    {
        //public RoleName Role { get; set; }
        //public SubscriptionType Subscription { get; set; }
        public CustomerTypeEnum CustomerType { get; set; }
        public decimal Amount { get; set; }
    }
}
