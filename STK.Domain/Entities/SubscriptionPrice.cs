
namespace STK.Domain.Entities
{
    public enum SubscriptionPriceCategory
    {
        Base = 1,
        AiRequests = 2
    }

    public class SubscriptionPrice
    {
        public Guid Id { get; set; }
        public SubscriptionPriceCategory Category { get; set; }
        public string Code { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int? DurationInMonths { get; set; }
        public int? RequestCount { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
