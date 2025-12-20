using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using STK.Application.DTOs.AuthDto;
using STK.Application.Handlers;
using STK.Application.Middleware;
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
        private readonly DataContext _dataContext;
        private readonly IMediator _mediator;
        private readonly ILogger<LegalSubscriptionUpdateService> _logger;
        private readonly ISubscriptionPriceProvider _subscriptionPriceProvider;

        public LegalSubscriptionUpdateService(DataContext dataContext, IMediator mediator, ILogger<LegalSubscriptionUpdateService> logger,
            ISubscriptionPriceProvider subscriptionPriceProvider)
        {
            _dataContext = dataContext;
            _mediator = mediator;
            _logger = logger;
            _subscriptionPriceProvider = subscriptionPriceProvider;
        }

        public async Task<string> ProcessAsync(User user, UpdateSubscriptionDto dto, CancellationToken cancellationToken)
        {
            var legal = await _dataContext.LegalRegistrations.AsNoTracking()
                .FirstOrDefaultAsync(l => l.UserId == user.Id, cancellationToken);

            var subscriptionPrice = dto.IsAdditionalFeature
                ? await _subscriptionPriceProvider.GetAiRequestsPriceAsync(dto.CountRequestAI, cancellationToken)
                : await _subscriptionPriceProvider.GetBasePriceAsync(dto.Subscription ?? throw DomainException.BadRequest("Не указан тип подписки"), cancellationToken);

            string submissionNumber = GenerateSubmissionNumber();

            var legalSubmis = new LegalSubmission
            {
                Id = Guid.NewGuid(),
                SubmissionNumber = submissionNumber,
                TypeSubmission = "update",
                LegalRegistrationId = legal.Id
            };

            _dataContext.LegalSubmissions.Add(legalSubmis);

            await _dataContext.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new LegalUserSubscriptionUpdatedEvent(
                    user.Email,
                    legal?.OrganizationName ?? string.Empty,
                    legal?.INN ?? string.Empty,
                    legal?.KPP ?? string.Empty,
                    legal?.OGRN ?? string.Empty,
                    legal?.Address ?? string.Empty,
                    legal?.Phone ?? string.Empty,
                    submissionNumber,
                    dto.Subscription,
                    dto.IsAdditionalFeature,
                    subscriptionPrice.RequestCount,
                    DateTime.UtcNow), cancellationToken);

            _logger.LogInformation("Subscription update event published for legal user {UserId}", user.Id);
            return string.Empty;
        }

        private string GenerateSubmissionNumber()
        {
            return $"REQ-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(10000, 99999)}";
        }
    }
}
