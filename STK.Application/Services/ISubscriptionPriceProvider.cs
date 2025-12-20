using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using STK.Domain.Entities;
using STK.Persistance;

namespace STK.Application.Services
{
    public interface ISubscriptionPriceProvider
    {
        Task<SubscriptionPrice> GetBasePriceAsync(SubscriptionType subscriptionType, CancellationToken cancellationToken);
        Task<SubscriptionPrice> GetAiRequestsPriceAsync(int requestCount, CancellationToken cancellationToken);
    }

    public class SubscriptionPriceProvider : ISubscriptionPriceProvider
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<SubscriptionPriceProvider> _logger;

        public SubscriptionPriceProvider(DataContext dataContext, ILogger<SubscriptionPriceProvider> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<SubscriptionPrice> GetBasePriceAsync(SubscriptionType subscriptionType, CancellationToken cancellationToken)
        {
            var code = subscriptionType.ToString().ToLower();
            var price = await _dataContext.SubscriptionPrices
                .Where(sp => sp.Category == SubscriptionPriceCategory.Base
                             && sp.Code.ToLower() == code
                             && sp.IsActive)
                .OrderByDescending(sp => sp.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (price == null)
            {
                _logger.LogWarning("Base subscription price not found for type {SubscriptionType}", subscriptionType);
                throw DomainException.BadRequest("Стоимость основной подписки не найдена. Обратитесь в поддержку.");
            }

            return price;
        }

        public async Task<SubscriptionPrice> GetAiRequestsPriceAsync(int requestCount, CancellationToken cancellationToken)
        {
            var price = await _dataContext.SubscriptionPrices
                .Where(sp => sp.Category == SubscriptionPriceCategory.AiRequests
                             && sp.RequestCount == requestCount
                             && sp.IsActive)
                .OrderByDescending(sp => sp.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (price == null)
            {
                _logger.LogWarning("AI request package price not found for count {RequestCount}", requestCount);
                throw DomainException.BadRequest("Стоимость пакета запросов не найдена. Обратитесь в поддержку.");
            }

            return price;
        }
    }
}
