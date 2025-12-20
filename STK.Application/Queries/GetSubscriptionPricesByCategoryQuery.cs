using MediatR;
using STK.Application.DTOs.Subscription;
using STK.Domain.Entities;


namespace STK.Application.Queries
{
    public class GetSubscriptionPricesByCategoryQuery : IRequest<List<SubscriptionPriceDto>>
    {
        public SubscriptionPriceCategory Category { get; set; }
    }
}
