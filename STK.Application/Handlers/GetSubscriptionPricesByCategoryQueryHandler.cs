using MediatR;
using STK.Application.DTOs.Subscription;
using STK.Application.Queries;
using STK.Application.Services;


namespace STK.Application.Handlers
{
    public class GetSubscriptionPricesByCategoryQueryHandler : IRequestHandler<GetSubscriptionPricesByCategoryQuery, List<SubscriptionPriceDto>>
    {
        private readonly ISubscriptionPriceProvider _subscriptionPriceProvider;

        public GetSubscriptionPricesByCategoryQueryHandler(ISubscriptionPriceProvider subscriptionPriceProvider)
        {
            _subscriptionPriceProvider = subscriptionPriceProvider;
        }

        public async Task<List<SubscriptionPriceDto>> Handle(GetSubscriptionPricesByCategoryQuery request, CancellationToken cancellationToken)
        {
            return await _subscriptionPriceProvider.GetPricesByCategoryAsync(request.Category, cancellationToken);
        }
    }
}
