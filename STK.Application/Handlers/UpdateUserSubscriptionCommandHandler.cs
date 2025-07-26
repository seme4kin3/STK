using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using STK.Application.Services;
using STK.Domain.Entities;
using STK.Persistance;

namespace STK.Application.Handlers
{
    //public record UpdateUserSubscriptionCommand(UpdateSubscriptionDto UpdateSubscriptionDto) : IRequest<bool>;
    //public class UpdateUserSubscriptionCommandHandler : IRequestHandler<UpdateUserSubscriptionCommand, bool>
    //{
    //    private readonly DataContext _dataContext;
    //    private readonly TBankPaymentService _payment;

    //    public UpdateUserSubscriptionCommandHandler(DataContext dataContext, TBankPaymentService payment)
    //    {
    //        _dataContext = dataContext;
    //        _payment = payment;
    //    }


    //    public async Task<bool> Handle(UpdateUserSubscriptionCommand command, CancellationToken cancellationToken)
    //    {
    //        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == command.UpdateSubscriptionDto.Email, cancellationToken);
    //        if (user == null)
    //        {
    //            throw new DomainException("User not found.", 204);
    //        }

    //        var dateOfUpdate = DateTime.UtcNow;
    //        var dto = command.UpdateSubscriptionDto;

    //        if (dto.IsAdditionalFeature)
    //        {
    //            await UpdateAdditionalFeature(user, dto, dateOfUpdate);
    //        }
    //        else
    //        {
    //            await UpdateMainSubscription(user, dto, dateOfUpdate);
    //        }

    //        await _dataContext.SaveChangesAsync(cancellationToken);

    //        return true;
    //    }

    //    private async Task UpdateMainSubscription(User user, UpdateSubscriptionDto dto, DateTime dateOfUpdate)
    //    {
    //        user.IsActive = true;
    //        user.UpdatedAt = dateOfUpdate;

    //        // Устанавливаем дату окончания подписки: текущая дата + 30 дней
    //        if(dto.Subscription == SubscriptionType.BaseQuarter)
    //        {
    //            user.SubscriptionEndTime = dateOfUpdate.AddDays(90);
    //        }
    //        else
    //        {
    //            user.SubscriptionEndTime = dateOfUpdate.AddDays(360);
    //        }

    //        user.SubscriptionType = dto.Subscription.ToString().ToLower();

    //        user.CountRequestAI = dto.CountRequestAI;

    //        _dataContext.Users.Update(user);

    //        var orderId = Guid.NewGuid().ToString();
    //        //var notificationUrl = $"https://lbzw3n2sr.localto.net/api/payment-callback";
    //        var amount = GetInitialRequestCount(dto.Subscription); 
    //        var payment = await _payment.InitPaymentAsync(orderId, amount, "Доступ к сервису", user.Email);

    //        var payRequest = new PaymentRequest
    //        {
    //            Id = Guid.Parse(orderId),
    //            UserId = user.Id,
    //            Amount = amount,
    //            PaymentUrl = payment.PaymentURL,
    //            PaymentId = payment.PaymentId,
    //            CreatedAt = DateTime.UtcNow,
    //            Description = "Продление основной подписки"
    //        };

    //        _dataContext.PaymentRequests.Add(payRequest);
    //    }

    //    private async Task UpdateAdditionalFeature(User user, UpdateSubscriptionDto dto, DateTime dateOfUpdate)
    //    {
    //        user.CountRequestAI += dto.CountRequestAI;
    //        user.UpdatedAt = dateOfUpdate;

    //        _dataContext.Users.Update(user);

    //        var orderId = Guid.NewGuid().ToString();
    //        //var notificationUrl = $"https://lbzw3n2sr.localto.net/api/payment-callback";
    //        var amount = GetAmountFromRequest(dto.CountRequestAI);
    //        var payment = await _payment.InitPaymentAsync(orderId, amount, "Дополнительные запросы к сервису AI", user.Email);

    //        var payRequest = new PaymentRequest
    //        {
    //            Id = Guid.Parse(orderId),
    //            UserId = user.Id,
    //            Amount = amount,
    //            PaymentUrl = payment.PaymentURL,
    //            PaymentId = payment.PaymentId,
    //            CreatedAt = DateTime.UtcNow,
    //            Description = "Дополнительные запросы к сервису AI"
    //        };

    //        _dataContext.PaymentRequests.Add(payRequest);
    //    }

    //    private int GetInitialRequestCount(SubscriptionType? subscriptionType)
    //    {
    //        return subscriptionType switch
    //        {
    //            SubscriptionType.BaseQuarter => 30000,
    //            SubscriptionType.BaseYear => 60000,
    //            _ => throw new ArgumentOutOfRangeException(nameof(subscriptionType), "Некорректно задан тип подписки.")
    //        };
    //    }

    //    private int GetAmountFromRequest(int countRequest)
    //    {
    //        return countRequest switch
    //        {
    //            30 => 4900,
    //            100 => 13900,
    //            300 => 34900,
    //            _ => throw new ArgumentOutOfRangeException(nameof(countRequest), "Некорректно задано число запросов.")
    //        };
    //    }
    //}

    public record UpdateUserSubscriptionCommand(UpdateSubscriptionDto Dto) : IRequest<string>;

    public class UpdateUserSubscriptionCommandHandler : IRequestHandler<UpdateUserSubscriptionCommand, string>
    {
        private readonly DataContext _db;
        private readonly TBankPaymentService _payment;
        private readonly ILogger<UpdateUserSubscriptionCommandHandler> _logger;

        public UpdateUserSubscriptionCommandHandler(DataContext db, TBankPaymentService payment, ILogger<UpdateUserSubscriptionCommandHandler> logger)
        {
            _db = db;
            _payment = payment;
            _logger = logger;
        }

        public async Task<string> Handle(UpdateUserSubscriptionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting subscription update for user {Email}. Request: {@Dto}", command.Dto.Email, command.Dto);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == command.Dto.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for email {Email}", command.Dto.Email);
                throw new DomainException("User not found.", 404);
            }

            var orderId = Guid.NewGuid();
            var amount = GetAmountForDto(command.Dto);

            try
            {
                var paymentResp = await _payment.InitPaymentAsync(orderId.ToString(), amount,
                    command.Dto.IsAdditionalFeature ? "Дополнительные запросы к сервису AI" : "Продление основной подписки", user.Email);

                var payRequest = new PaymentRequest
                {
                    Id = orderId,
                    UserId = user.Id,
                    Amount = amount,
                    PaymentUrl = paymentResp.PaymentURL,
                    PaymentId = paymentResp.PaymentId,
                    CreatedAt = DateTime.UtcNow,
                    Description = command.Dto.IsAdditionalFeature ? "Дополнительные запросы к сервису AI" : "Продление основной подписки"
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

        private int GetAmountForDto(UpdateSubscriptionDto dto)
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
