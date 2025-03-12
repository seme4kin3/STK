
namespace STK.Domain.Entities
{
    public class UserFavoriteOrganization
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public DateTime DateAddedOn { get; set; }
    }
}
