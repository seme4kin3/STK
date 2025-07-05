using MediatR;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public record DecrementUserCountRequestAiCommand(Guid UserId) : IRequest<int>;
    public class DecrementUserCountRequestAiCommandHandler : IRequestHandler<DecrementUserCountRequestAiCommand, int>
    {
        private readonly DataContext _dataContext;

        public DecrementUserCountRequestAiCommandHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<int> Handle(DecrementUserCountRequestAiCommand command, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FindAsync(command.UserId, cancellationToken);
            if (user == null)
                throw new Exception("User not found");

            if (user.CountRequestAI <= 0)
                throw new InvalidOperationException("CountRequestAi cannot be negative");

            user.CountRequestAI--;

            int result = user.CountRequestAI ?? 0;
            await _dataContext.SaveChangesAsync(cancellationToken);


            return result;
        }
    }
}
