using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.Commands;
using STK.Domain.Entities;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class PaymentCallbackCommandHandler : IRequestHandler<PaymentCallbackCommand, Unit>
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<PaymentCallbackCommandHandler> _logger;
        private const int QuarterAmount = 30000;
        private const int YearAmount = 60000;
        private const int Extra30Amount = 4900;
        private const int Extra100Amount = 13900;
        private const int Extra300Amount = 34900;
        public PaymentCallbackCommandHandler(DataContext dataContext, ILogger<PaymentCallbackCommandHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        //public async Task<Unit> Handle(PaymentCallbackCommand request, CancellationToken cancellationToken)
        //{
        //    var payReq = await _dataContext.PaymentRequests.FindAsync(request.OrderId);
        //    var timeUpdate = DateTime.UtcNow;

        //    if (payReq == null)
        //        throw new Exception("Payment request not found");

        //    if (request.Status == "REJECTED") // Обработка отклоненного платежа
        //    {
        //        payReq.Description = "REJECTED";
        //        payReq.CompletedAt = timeUpdate;
        //        payReq.IsPaid = false;
        //        await _dataContext.SaveChangesAsync(cancellationToken);
        //        return Unit.Value;
        //    }

        //    if (request.Success && request.Status == "CONFIRMED" && !payReq.IsPaid) // Обработка успешного платежа
        //    {
        //        payReq.IsPaid = true;
        //        payReq.Description = "CONFIRMED";
        //        payReq.CompletedAt = timeUpdate;

        //        var user = await _dataContext.Users
        //            .Include(u => u.UserRoles)
        //                .ThenInclude(ur => ur.Role)
        //            .FirstOrDefaultAsync(u => u.Id == payReq.UserId, cancellationToken);

        //        if (user == null)
        //            throw new Exception("User not found");

        //        user.UpdatedAt = timeUpdate;
        //        user.IsActive = true;

        //        user.SubscriptionEndTime = payReq.Amount switch
        //        {
        //            30000 => (user.SubscriptionEndTime ?? DateTime.UtcNow).AddMonths(3),
        //            60000 => (user.SubscriptionEndTime ?? DateTime.UtcNow).AddYears(1),
        //            _ => user.SubscriptionEndTime 
        //        };

        //        var targetRole = await _dataContext.Roles
        //            .FirstOrDefaultAsync(r => r.Name == "user", cancellationToken);

        //        if (targetRole == null)
        //            throw new Exception("Role 'user' not found in database");

        //        // Удаляем существующие роли пользователя
        //        foreach (var ur in user.UserRoles.ToList())
        //        {
        //            _dataContext.UserRoles.Remove(ur);
        //        }

        //        // Добавляем новую связь с ролью 'user'
        //        _dataContext.UserRoles.Add(new UserRole
        //        {
        //            UserId = user.Id,
        //            RoleId = targetRole.Id
        //        });

        //        await _dataContext.SaveChangesAsync(cancellationToken);
        //    }
        //    return Unit.Value;
        //}

        public async Task<Unit> Handle(PaymentCallbackCommand request, CancellationToken cancellationToken)
        {
            var payReq = await _dataContext.PaymentRequests.FindAsync(request.OrderId);
            if (payReq == null)
            {
                _logger.LogWarning("Payment request not found: {OrderId}", request.OrderId);
                throw new Exception("Payment request not found");
            }

            var user = await _dataContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == payReq.UserId, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", payReq.UserId);
                throw new Exception("User not found");
            }

            var timeUpdate = DateTime.UtcNow;
            payReq.Description = request.Status;
            payReq.CompletedAt = timeUpdate;

            // Только payReq.IsPaid == false для идемпотентности
            if (request.Success && request.Status == "CONFIRMED" && !payReq.IsPaid)
            {
                payReq.IsPaid = true;
                user.UpdatedAt = timeUpdate;
                user.IsActive = true;

                // Основные подписки
                if (payReq.Amount == QuarterAmount)
                {
                    user.SubscriptionType = "basequarter";
                    user.SubscriptionEndTime = (user.SubscriptionEndTime ?? timeUpdate) > timeUpdate
                        ? user.SubscriptionEndTime.Value.AddMonths(3)
                        : timeUpdate.AddMonths(3);
                    user.CountRequestAI += 3;
                }
                else if (payReq.Amount == YearAmount)
                {
                    user.SubscriptionType = "baseyear";
                    user.SubscriptionEndTime = (user.SubscriptionEndTime ?? timeUpdate) > timeUpdate
                        ? user.SubscriptionEndTime.Value.AddYears(1)
                        : timeUpdate.AddYears(1);
                    user.CountRequestAI += 3;
                }
                // Покупка дополнительных запросов — расширяем лимит, не меняем подписку
                else if (payReq.Amount == Extra30Amount)
                {
                    user.CountRequestAI += 30;
                }
                else if (payReq.Amount == Extra100Amount)
                {
                    user.CountRequestAI += 100;
                }
                else if (payReq.Amount == Extra300Amount)
                {
                    user.CountRequestAI += 300;
                }

                // Обновление ролей: оставить только "user"
                var targetRole = await _dataContext.Roles.FirstOrDefaultAsync(r => r.Name == "user", cancellationToken)
                ?? throw new Exception("Role 'user' not found");
                _dataContext.UserRoles.RemoveRange(user.UserRoles);
                _dataContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = targetRole.Id });

                _logger.LogInformation("Payment confirmed and user updated: {UserId}, amount: {Amount}", user.Id, payReq.Amount);
            }
            else if (request.Status == "REJECTED")
            {
                payReq.IsPaid = false;
                payReq.Description = "REJECTED";
                user.IsActive = false; // при необходимости
                _logger.LogWarning("Payment rejected: {OrderId}", request.OrderId);
            }
            // Можно добавить обработку CANCELED, REFUNDED, PARTIAL_REFUNDED

            await _dataContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
