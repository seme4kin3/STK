using System.ComponentModel.DataAnnotations;


namespace STK.Application.DTOs.AuthDto
{
    public class UserInfoDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public Guid UserId { get; set; }
        public SubscriptionType Subscription { get; set; }
        public int CountRequest { get; set; }
    }
}
