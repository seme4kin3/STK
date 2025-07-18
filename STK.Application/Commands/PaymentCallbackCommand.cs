using MediatR;

namespace STK.Application.Commands
{
    public class PaymentCallbackCommand : IRequest<Unit>
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; }
        public bool Success { get; set; }
    }
}
