using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Queries;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetPaymentUrlQueryHandler : IRequestHandler<GetPaymentUrlQuery, string>
    {
        private readonly DataContext _dataContext;
        public GetPaymentUrlQueryHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<string> Handle(GetPaymentUrlQuery request, CancellationToken cancellationToken)
        {
            var payReq = await _dataContext.PaymentRequests
                .Where(x => x.UserId == request.UserId && !x.IsPaid)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            return payReq?.PaymentUrl;
        }
    }
}
