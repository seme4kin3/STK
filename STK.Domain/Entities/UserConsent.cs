
namespace STK.Domain.Entities
{
    public class UserConsent
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? DocumentVersion { get; set; }
        public string? DocumentUrl { get; set; }
        public DateTime AcceptedAt { get; set; }
        public string IpAddress { get; set; }
        public bool IsAccepted { get; set; }
        public User User { get; set; }
    }
}
