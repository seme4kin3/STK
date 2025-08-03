using Microsoft.Extensions.Logging;
using STK.Application.DTOs.AuthDto;
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

        public IndividualSubscriptionUpdateService(DataContext db, TBankPaymentService payment, ILogger<IndividualSubscriptionUpdateService> logger)
        {
            _db = db;
            _payment = payment;
            _logger = logger;
        }

        public async Task<string> ProcessAsync(User user, UpdateSubscriptionDto dto, CancellationToken cancellationToken)
        {
            var orderId = Guid.NewGuid();
            var amount = GetAmount(dto);

            try
            {
                var paymentResp = await _payment.InitPaymentAsync(orderId.ToString(), amount,
                dto.IsAdditionalFeature ? "Дополнительные запросы к сервису AI" : "Продление основной подписки", user.Email);

                var payRequest = new PaymentRequest
                {
                    Id = orderId,
                    UserId = user.Id,
                    Amount = amount,
                    PaymentUrl = paymentResp.PaymentURL,
                    PaymentId = paymentResp.PaymentId,
                    CreatedAt = DateTime.UtcNow,
                    Description = dto.IsAdditionalFeature ? "Дополнительные запросы к сервису AI" : "Продление основной подписки",
                };

                _db.PaymentRequests.Add(payRequest);
                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created PaymentRequest for user {UserId}. OrderId: {OrderId}, Amount: {Amount}, Description: {Desc}, PaymentId: {PaymentId}",
                user.Id, orderId, amount, payRequest.Description, payRequest.PaymentId);

                return paymentResp.PaymentURL;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during subscription update/payment init for user {UserId}. OrderId: {OrderId}, Amount: {Amount}", user.Id, orderId, amount);
                throw;
            }
        }

        private int GetAmount(UpdateSubscriptionDto dto)
        {
            if (dto.IsAdditionalFeature)
            {
                return dto.CountRequestAI switch
                {
                    30 => 4900,
                    100 => 13900,
                    300 => 34900,
                    _ => throw new ArgumentOutOfRangeException(nameof(dto.CountRequestAI), "Некорректное количество доп. запросов.")
                };
            }

            return dto.Subscription switch
            {
                SubscriptionType.BaseQuarter => 30000,
                SubscriptionType.BaseYear => 60000,
                _ => throw new ArgumentOutOfRangeException(nameof(dto.Subscription), "Некорректный тип подписки.")
            };
        }
    }
}
