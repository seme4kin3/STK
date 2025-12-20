
namespace STK.Application.DTOs.Subscription
{
    public class SubscriptionPriceDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public int? DurationInMonths { get; set; }
        public int? RequestCount { get; set; }
        public decimal? Price { get; set; }
    }
}
