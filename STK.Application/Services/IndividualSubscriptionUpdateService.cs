using Microsoft.Extensions.Logging;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using STK.Domain.Entities;
using STK.Persistance;

namespace STK.Application.Services
{
    public interface IIndividualSubscriptionUpdateService
    {
        Task<string> ProcessAsync(User user, UpdateSubscriptionDto dto, CancellationToken cancellationToken);
    }

    public class IndividualSubscriptionUpdateService : IIndividualSubscriptionUpdateService
    {
        private readonly DataContext _db;
        private readonly TBankPaymentService _payment;
        private readonly ILogger<IndividualSubscriptionUpdateService> _logger;
        private readonly ISubscriptionPriceProvider _subscriptionPriceProvider;

        public IndividualSubscriptionUpdateService(DataContext db, TBankPaymentService payment, ILogger<IndividualSubscriptionUpdateService> logger,
            ISubscriptionPriceProvider subscriptionPriceProvider)
        {
            _db = db;
            _payment = payment;
            _logger = logger;
            _subscriptionPriceProvider = subscriptionPriceProvider;
        }

        public async Task<string> ProcessAsync(User user, UpdateSubscriptionDto dto, CancellationToken cancellationToken)
        {
            var orderId = Guid.NewGuid();
            var price = dto.IsAdditionalFeature
                ? await _subscriptionPriceProvider.GetAiRequestsPriceAsync(dto.CountRequestAI, cancellationToken)
                : await _subscriptionPriceProvider.GetBasePriceAsync(dto.Subscription ?? throw DomainException.BadRequest("Не указан тип подписки"), cancellationToken);

            try
            {
                var paymentResp = await _payment.InitPaymentAsync(orderId.ToString(), price.Price,
                    price.Description, user.Email);

                var payRequest = new PaymentRequest
                {
                    Id = orderId,
                    UserId = user.Id,
                    Amount = price.Price,
                    PaymentUrl = paymentResp.PaymentURL,
                    PaymentId = paymentResp.PaymentId,
                    CreatedAt = DateTime.UtcNow,
                    Description = price.Description,
                    SubscriptionPriceId = price.Id
                };

                _db.PaymentRequests.Add(payRequest);
                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created PaymentRequest for user {UserId}. OrderId: {OrderId}, Amount: {Amount}, Description: {Desc}, PaymentId: {PaymentId}",
                user.Id, orderId, price.Price, payRequest.Description, payRequest.PaymentId);

                return paymentResp.PaymentURL;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during subscription update/payment init for user {UserId}. OrderId: {OrderId}, Amount: {Amount}", user.Id, orderId, price.Price);
                throw;
            }
        }
    }
}
