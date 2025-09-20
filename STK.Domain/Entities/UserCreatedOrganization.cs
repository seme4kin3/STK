
namespace STK.Domain.Entities
{
    public class UserCreatedOrganization
    {
        public Guid UserId { get; set; }
        public Guid OrganizationId { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public User User { get; set; }
        public Organization Organization { get; set; }
    }
}
