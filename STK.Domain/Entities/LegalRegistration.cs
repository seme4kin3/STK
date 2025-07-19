
namespace STK.Domain.Entities
{
    public class LegalRegistration
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string OrganizationName { get; set; }
        public string INN { get; set; }
        public string KPP { get; set; }
        public string OGRN { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
    }
}
