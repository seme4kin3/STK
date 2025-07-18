using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using STK.Application.Services;
using STK.Domain.Entities;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public record UpdateUserSubscriptionCommand(UpdateSubscriptionDto UpdateSubscriptionDto) : IRequest<bool>;
    public class UpdateUserSubscriptionCommandHandler : IRequestHandler<UpdateUserSubscriptionCommand, bool>
    {
        private readonly DataContext _dataContext;
        private readonly TBankPaymentService _payment;

        public UpdateUserSubscriptionCommandHandler(DataContext dataContext, TBankPaymentService payment)
        {
            _dataContext = dataContext;
            _payment = payment;
        }


        public async Task<bool> Handle(UpdateUserSubscriptionCommand command, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == command.UpdateSubscriptionDto.Email, cancellationToken);
            if (user == null)
            {
                throw new DomainException("User not found.", 204);
            }

            var dateOfUpdate = DateTime.UtcNow;
            var dto = command.UpdateSubscriptionDto;

            if (dto.IsAdditionalFeature)
            {
                await UpdateAdditionalFeature(user, dto, dateOfUpdate);
            }
            else
            {
                await UpdateMainSubscription(user, dto, dateOfUpdate);
            }

            await _dataContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        private async Task UpdateMainSubscription(User user, UpdateSubscriptionDto dto, DateTime dateOfUpdate)
        {
            user.IsActive = true;
            user.UpdatedAt = dateOfUpdate;

            // Устанавливаем дату окончания подписки: текущая дата + 30 дней
            if(dto.Subscription == SubscriptionType.BaseQuarter)
            {
                user.SubscriptionEndTime = dateOfUpdate.AddDays(90);
            }
            else
            {
                user.SubscriptionEndTime = dateOfUpdate.AddDays(360);
            }

            user.SubscriptionType = dto.Subscription.ToString().ToLower();

            user.CountRequestAI = dto.CountRequestAI;

            _dataContext.Users.Update(user);

            var orderId = Guid.NewGuid().ToString();
            var notificationUrl = $"https://lbzw3n2sr.localto.net/api/payment-callback";
            var amount = GetInitialRequestCount(dto.Subscription); 
            var payment = await _payment.InitPaymentAsync(orderId, amount, "Доступ к сервису", notificationUrl, user.Email);

            var payRequest = new PaymentRequest
            {
                Id = Guid.Parse(orderId),
                UserId = user.Id,
                Amount = amount,
                PaymentUrl = payment.PaymentURL,
                PaymentId = payment.PaymentId,
                CreatedAt = DateTime.UtcNow,
                Description = "Продление основной подписки"
            };

            _dataContext.PaymentRequests.Add(payRequest);
        }

        private async Task UpdateAdditionalFeature(User user, UpdateSubscriptionDto dto, DateTime dateOfUpdate)
        {
            user.CountRequestAI += dto.CountRequestAI;
            user.UpdatedAt = dateOfUpdate;

            _dataContext.Users.Update(user);

            var orderId = Guid.NewGuid().ToString();
            var notificationUrl = $"https://lbzw3n2sr.localto.net/api/payment-callback";
            var amount = GetAmountFromRequest(dto.CountRequestAI);
            var payment = await _payment.InitPaymentAsync(orderId, amount, "Дополнительные запросы к сервису AI", notificationUrl, user.Email);

            var payRequest = new PaymentRequest
            {
                Id = Guid.Parse(orderId),
                UserId = user.Id,
                Amount = amount,
                PaymentUrl = payment.PaymentURL,
                PaymentId = payment.PaymentId,
                CreatedAt = DateTime.UtcNow,
                Description = "Дополнительные запросы к сервису AI"
            };

            _dataContext.PaymentRequests.Add(payRequest);
        }

        private int GetInitialRequestCount(SubscriptionType? subscriptionType)
        {
            return subscriptionType switch
            {
                SubscriptionType.BaseQuarter => 30000,
                SubscriptionType.BaseYear => 60000,
                _ => throw new ArgumentOutOfRangeException(nameof(subscriptionType), "Некорректно задан тип подписки.")
            };
        }

        private int GetAmountFromRequest(int countRequest)
        {
            return countRequest switch
            {
                30 => 4900,
                100 => 13900,
                300 => 34900,
                _ => throw new ArgumentOutOfRangeException(nameof(countRequest), "Некорректно задано число запросов.")
            };
        }
    }
}
