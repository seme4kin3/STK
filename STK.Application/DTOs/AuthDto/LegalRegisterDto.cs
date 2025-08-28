using System.ComponentModel.DataAnnotations;

namespace STK.Application.DTOs.AuthDto
{
    public class LegalRegisterDto : BaseUserDto
    {
        [Required]
        public string OrganizationName { get; set; }
        [Required]
        public string INN { get; set; }
        public string KPP { get; set; }
        public string OGRN { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        [Required]
        public SubscriptionType SubscriptionType { get; set; }
        public bool IsAccepted { get; set; }
        public string? OfferVersion { get; set; }
        public string? OfferLink { get; set; }
    }
}
