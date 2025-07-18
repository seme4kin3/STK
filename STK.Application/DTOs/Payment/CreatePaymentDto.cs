

namespace STK.Application.DTOs.Payment
{
    public class CreatePaymentDto
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }

    }
}
