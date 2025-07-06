using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.DTOs.AuthDtoTest;
using STK.Application.Middleware;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public record UpdateUserSubscriptionCommand(UpdateSubscriptionDto UpdateSubscriptionDto) : IRequest<DateTime>;
    public class UpdateUserSubscriptionCommandHandler : IRequestHandler<UpdateUserSubscriptionCommand, DateTime>
    {
        private readonly DataContext _dataContext;

        public UpdateUserSubscriptionCommandHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        //public async Task<DateTime> Handle(UpdateUserSubscriptionCommand command, CancellationToken cancellationToken)
        //{
        //    var user = await _dataContext.Users.FindAsync(command.UserEmail, cancellationToken);
        //    if (user == null)
        //        throw new Exception("User not found");

        //    var dateOfUpdate = DateTime.UtcNow;

        //    user.SubscriptionType = command.SubscriptionType;
        //    user.UpdatedAt = dateOfUpdate;

        //    user.CountRequestAI += command.SubscriptionType switch
        //    {
        //        "nosubscription" => 0,
        //        "free" => 0,
        //        "standard" => 3,
        //        "premium" => 15,
        //        _ => throw new InvalidOperationException("Invalid subscription type")
        //    };

        //    await _dataContext.SaveChangesAsync(cancellationToken);

        //    return dateOfUpdate;
        //}

        public async Task<DateTime> Handle(UpdateUserSubscriptionCommand command, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == command.UpdateSubscriptionDto.Email, cancellationToken);
            if (user == null)
            {
                throw new DomainException("User not found.", 204);
            }

            var dateOfUpdate = DateTime.UtcNow;

            user.SubscriptionType = command.UpdateSubscriptionDto.Subscription.ToString().ToLower();
            user.UpdatedAt = dateOfUpdate;
            user.IsActive = true; // Активируем пользователя при обновлении подписки

            user.CountRequestAI = (user.CountRequestAI ?? 0) + GetRequestCount(command.UpdateSubscriptionDto.Subscription);

            await _dataContext.SaveChangesAsync(cancellationToken);

            return dateOfUpdate;
        }

        private int GetRequestCount(SubscriptionType subscriptionType)
        {
            return subscriptionType switch
            {
                SubscriptionType.NoSubscription => 0,
                SubscriptionType.Free => 0,
                SubscriptionType.Standard => 3,
                SubscriptionType.Premium => 15,
                _ => throw new InvalidOperationException("Invalid subscription type")
            };
        }
    }
}
