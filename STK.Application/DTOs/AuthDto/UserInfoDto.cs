using System.ComponentModel.DataAnnotations;


namespace STK.Application.DTOs.AuthDto
{
    public class UserInfoDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public Guid UserId { get; set; }
        public string SubscriptionType { get; set; }
        public int CountRequest { get; set; }
        public DateTime? SubscriptionEndTime { get; set; }
        public string CustomerType { get; set; }
        public bool IsActive { get; set; }
    }
}
