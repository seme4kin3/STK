using MediatR;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public record UpdateUserSubscriptionCommand(string UserEmail, string SubscriptionType) : IRequest<DateTime>;
    public class UpdateUserSubscriptionCommandHandler : IRequestHandler<UpdateUserSubscriptionCommand, DateTime>
    {
        private readonly DataContext _dataContext;

        public UpdateUserSubscriptionCommandHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<DateTime> Handle(UpdateUserSubscriptionCommand command, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FindAsync(command.UserEmail, cancellationToken);
            if (user == null)
                throw new Exception("User not found");

            var dateOfUpdate = DateTime.UtcNow;

            user.SubscriptionType = command.SubscriptionType;
            user.UpdatedAt = dateOfUpdate;

            user.CountRequestAI += command.SubscriptionType switch
            {
                "nosubscription" => 0,
                "free" => 0,
                "standard" => 3,
                "premium" => 15,
                _ => throw new InvalidOperationException("Invalid subscription type")
            };

            await _dataContext.SaveChangesAsync(cancellationToken);

            return dateOfUpdate;
        }
    }
}
