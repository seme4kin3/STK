using System.ComponentModel.DataAnnotations;

namespace STK.Application.DTOs.AuthDtoTest
{
    public class UpdateSubscriptionDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public SubscriptionType Subscription { get; set; }
    }
}
