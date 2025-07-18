
namespace STK.Domain.Entities
{
    public class PaymentRequest
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentUrl { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PaymentId { get; set; }
        public DateTime CompletedAt { get; set; }
        public string Description { get; set; }
    }
}
