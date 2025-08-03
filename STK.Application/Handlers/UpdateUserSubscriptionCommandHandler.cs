using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using STK.Application.Services;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public record UpdateUserSubscriptionCommand(UpdateSubscriptionDto Dto) : IRequest<string>;

    public class UpdateUserSubscriptionCommandHandler : IRequestHandler<UpdateUserSubscriptionCommand, string>
    {
        private readonly DataContext _db;
        private readonly TBankPaymentService _payment;
        private readonly ILogger<UpdateUserSubscriptionCommandHandler> _logger;
        private readonly ILegalSubscriptionUpdateService _legalSubscriptionUpdateService;
        private readonly IIndividualSubscriptionUpdateService _individualSubscriptionUpdateService;

        public UpdateUserSubscriptionCommandHandler(DataContext db,
            TBankPaymentService payment,
            ILogger<UpdateUserSubscriptionCommandHandler> logger,
            ILegalSubscriptionUpdateService legalService,
            IIndividualSubscriptionUpdateService individualService)
        {
            _db = db;
            _payment = payment;
            _logger = logger;
            _legalSubscriptionUpdateService = legalService;
            _individualSubscriptionUpdateService = individualService;
        }

        public async Task<string> Handle(UpdateUserSubscriptionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Srating subscription update for user {Email}, Request {@Dto}", command.Dto.Email, command.Dto);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == command.Dto.Email, cancellationToken);

            if(user == null)
            {
                _logger.LogWarning("User not found {Email}", command.Dto.Email);
                throw DomainException.UserNotFound("Пользователь с таким email не найден");
            }

            if(string.Equals(user.CustomerType, CustomerTypeEnum.Legal.ToString().ToLower(), StringComparison.OrdinalIgnoreCase))
            {
                return await _legalSubscriptionUpdateService.ProcessAsync(user, command.Dto, cancellationToken);
            }

            return await _individualSubscriptionUpdateService.ProcessAsync(user, command.Dto, cancellationToken);
        }

    }
}
