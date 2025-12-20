
using System.ComponentModel.DataAnnotations;

namespace STK.Application.DTOs.AuthDto
{
    public class RegisterDto : BaseUserDto
    {
        [Required]
        public Guid SubscriptionPriceId { get; set; }
        public bool IsAccepted { get; set; }
        public string? OfferVersion { get; set; }
        public string? OfferLink { get; set; }

    }
}
