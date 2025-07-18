using MediatR;


namespace STK.Application.Queries
{
    public class GetPaymentUrlQuery : IRequest<string>
    {
        public Guid UserId { get; set; }
    }
}
