using MediatR;
using STK.Application.Commands;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class PaymentCallbackCommandHandler : IRequestHandler<PaymentCallbackCommand, Unit>
    {
        private readonly DataContext _dataContext;

        public PaymentCallbackCommandHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<Unit> Handle(PaymentCallbackCommand request, CancellationToken cancellationToken)
        {
            var payReq = await _dataContext.PaymentRequests.FindAsync(request.OrderId);
            if (payReq == null)
                throw new Exception("Payment request not found");

            if (request.Success && request.Status == "CONFIRMED" && !payReq.IsPaid)
            {
                payReq.IsPaid = true;
                var user = await _dataContext.Users.FindAsync(payReq.UserId);
                user.IsActive = true;
                await _dataContext.SaveChangesAsync();
            }
            return Unit.Value;
        }
    }

}
