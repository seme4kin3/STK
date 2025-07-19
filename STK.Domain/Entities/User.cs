
namespace STK.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? SubscriptionEndTime {get; set; }
        public bool IsActive { get; set; }   
        public string SubscriptionType { get; set; }
        public int? CountRequestAI { get; set; }
        public string CustomerType { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<UserFavoriteOrganization> FavoritesOrganizations { get; set; } = new List<UserFavoriteOrganization>();
        public ICollection<UserFavoriteCertificate> FavoritesCertificates { get; set;} = new List<UserFavoriteCertificate>();
    }
}
