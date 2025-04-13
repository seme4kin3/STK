
namespace STK.Domain.Entities
{
    public class UserFavoriteCertificate
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid CertificateId { get; set; }
        public Certificate Certificate { get; set; }
        public DateTime DateAddedOn { get; set; }
    }
}
