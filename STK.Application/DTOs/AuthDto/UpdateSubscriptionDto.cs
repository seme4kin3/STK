using System.ComponentModel.DataAnnotations;

namespace STK.Application.DTOs.AuthDto
{
    public class UpdateSubscriptionDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public SubscriptionType? Subscription { get; set; }
        public bool IsAdditionalFeature { get; set; }
        public int CountRequestAI { get; set; } = 3;
    }
}
