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
        private readonly DataContext _dataContext;
        private readonly IMediator _mediator;
        private readonly ILogger<LegalSubscriptionUpdateService> _logger;

        public LegalSubscriptionUpdateService(DataContext dataContext, IMediator mediator, ILogger<LegalSubscriptionUpdateService> logger)
        {
            _dataContext = dataContext;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<string> ProcessAsync(User user, UpdateSubscriptionDto dto, CancellationToken cancellationToken)
        {
            var legal = await _dataContext.LegalRegistrations.AsNoTracking()
                .FirstOrDefaultAsync(l => l.UserId == user.Id, cancellationToken);

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
                    dto.CountRequestAI,
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
