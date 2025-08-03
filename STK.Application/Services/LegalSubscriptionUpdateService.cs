using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.AuthDto;
using STK.Application.Handlers;
using STK.Domain.Entities;
using STK.Persistance;

namespace STK.Application.Services
{
    public interface ILegalSubscriptionUpdateService
    {
        Task<string> ProcessAsync(User user, UpdateSubscriptionDto dto, CancellationToken cancellationToken);
    }

    public class LegalSubscriptionUpdateService : ILegalSubscriptionUpdateService
    {
        private readonly DataContext _db;
        private readonly IMediator _mediator;
        private readonly ILogger<LegalSubscriptionUpdateService> _logger;

        public LegalSubscriptionUpdateService(DataContext db, IMediator mediator, ILogger<LegalSubscriptionUpdateService> logger)
        {
            _db = db;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<string> ProcessAsync(User user, UpdateSubscriptionDto dto, CancellationToken cancellationToken)
        {
            var legal = await _db.LegalRegistrations.AsNoTracking()
                .FirstOrDefaultAsync(l => l.UserId == user.Id, cancellationToken);

            await _mediator.Publish(new LegalUserSubscriptionUpdatedEvent(
                    user.Email,
                    legal?.OrganizationName ?? string.Empty,
                    legal?.INN ?? string.Empty,
                    legal?.KPP ?? string.Empty,
                    legal?.OGRN ?? string.Empty,
                    legal?.Address ?? string.Empty,
                    legal?.Phone ?? string.Empty,
                    dto.Subscription,
                    dto.IsAdditionalFeature,
                    dto.CountRequestAI,
                    DateTime.UtcNow), cancellationToken);

            _logger.LogInformation("Subscription update event published for legal user {UserId}", user.Id);
            return string.Empty;
        }
    }
}
