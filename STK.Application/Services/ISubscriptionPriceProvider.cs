using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.AuthDto;
using STK.Application.DTOs.Subscription;
using STK.Application.Middleware;
using STK.Domain.Entities;
using STK.Persistance;

namespace STK.Application.Services
{
    public interface ISubscriptionPriceProvider
    {
        Task<SubscriptionPrice> GetBasePriceAsync(SubscriptionType subscriptionType, CancellationToken cancellationToken);
        Task<SubscriptionPrice> GetAiRequestsPriceAsync(int requestCount, CancellationToken cancellationToken);
        Task<List<SubscriptionPriceDto>> GetPricesByCategoryAsync(SubscriptionPriceCategory category, CancellationToken cancellationToken);
        Task<SubscriptionPrice> GetPriceByIdAsync(Guid subscriptionPriceId, CancellationToken cancellationToken);
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

        public async Task<List<SubscriptionPriceDto>> GetPricesByCategoryAsync(SubscriptionPriceCategory category, CancellationToken cancellationToken)
        {
            var prices = await _dataContext.SubscriptionPrices
                .Where(sp => sp.Category == category && sp.IsActive)
                .OrderByDescending(sp => sp.UpdatedAt)
                .Select(sp => new SubscriptionPriceDto
                {
                    Id = sp.Id,
                    Description = sp.Description,
                    DurationInMonths = sp.DurationInMonths,
                    RequestCount = sp.RequestCount,
                    Price = sp.Price,
                })
                .ToListAsync(cancellationToken);

            if (!prices.Any())
            {
                _logger.LogWarning("No subscription prices found for category {Category}", category);
                throw DomainException.BadRequest("Стоимость для указанной категории не найдена. Обратитесь в поддержку.");
            }

            return prices;
        }

        public async Task<SubscriptionPrice> GetPriceByIdAsync(Guid subscriptionPriceId, CancellationToken cancellationToken)
        {
            var price = await _dataContext.SubscriptionPrices
                .Where(sp => sp.Id == subscriptionPriceId && sp.IsActive)
                .OrderByDescending(sp => sp.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (price == null)
            {
                _logger.LogWarning("Subscription price not found by id {SubscriptionPriceId}", subscriptionPriceId);
                throw DomainException.BadRequest("Стоимость подписки не найдена. Обратитесь в поддержку.");
            }

            return price;
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
