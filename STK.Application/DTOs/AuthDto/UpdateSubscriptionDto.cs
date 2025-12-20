using System.ComponentModel.DataAnnotations;

namespace STK.Application.DTOs.AuthDto
{
    public class UpdateSubscriptionDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public Guid SubscriptionPriceId { get; set; }
    }
}
