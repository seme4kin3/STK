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

        public PaymentCallbackCommandHandler(DataContext dataContext, ILogger<PaymentCallbackCommandHandler> logger)
        {
            _dataContext = dataContext;
            _logger = logger;
        }

        public async Task<Unit> Handle(PaymentCallbackCommand request, CancellationToken cancellationToken)
        {
            var payReq = await _dataContext.PaymentRequests
                .Include(pr => pr.SubscriptionPrice)
                .FirstOrDefaultAsync(pr => pr.Id == request.OrderId, cancellationToken);

            if (payReq == null)
            {
                _logger.LogWarning("Payment request not found: {OrderId}", request.OrderId);
                throw new Exception("Payment request not found");
            }

            var user = await _dataContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == payReq.UserId, cancellationToken);

            var price = payReq.SubscriptionPrice
                ?? (payReq.SubscriptionPriceId.HasValue
                    ? await _dataContext.SubscriptionPrices.FirstOrDefaultAsync(sp => sp.Id == payReq.SubscriptionPriceId.Value, cancellationToken)
                    : null)
                ?? await _dataContext.SubscriptionPrices
                    .Where(sp => sp.Price == payReq.Amount && sp.IsActive)
                    .OrderByDescending(sp => sp.UpdatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", payReq.UserId);
                throw new Exception("User not found");
            }

            if (price == null)
            {
                _logger.LogWarning("Subscription price not found for payment request {OrderId}", request.OrderId);
                throw new Exception("Subscription price not found");
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

                var currentEndTime = (user.SubscriptionEndTime ?? timeUpdate) > timeUpdate
                    ? user.SubscriptionEndTime.Value
                    : timeUpdate;

                if (price.Category == SubscriptionPriceCategory.Base)
                {
                    user.SubscriptionType = price.Code;
                    user.SubscriptionEndTime = price.DurationInMonths.HasValue && price.DurationInMonths.Value > 0
                        ? currentEndTime.AddMonths(price.DurationInMonths.Value)
                        : currentEndTime;
                    user.CountRequestAI = (user.CountRequestAI ?? 0) + price.RequestCount;
                }

                else if (price.Category == SubscriptionPriceCategory.AiRequests)
                {
                    user.CountRequestAI = (user.CountRequestAI ?? 0) + price.RequestCount;
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
