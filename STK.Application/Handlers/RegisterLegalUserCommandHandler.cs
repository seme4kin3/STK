using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using STK.Application.Services;
using STK.Domain.Entities;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class RegisterLegalUserCommandHandler : IRequestHandler<RegisterLegalUserCommand, Unit>
    {
        private readonly DataContext _dataContext;
        private readonly IMediator _mediator;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ISubscriptionPriceProvider _subscriptionPriceProvider;

        public RegisterLegalUserCommandHandler(DataContext dataContext, IMediator mediator,
            IPasswordHasher passwordHasher, ISubscriptionPriceProvider subscriptionPriceProvider)
        {
            _dataContext = dataContext;
            _mediator = mediator;
            _passwordHasher = passwordHasher;
            _subscriptionPriceProvider = subscriptionPriceProvider;
        }

        public async Task<Unit> Handle(RegisterLegalUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username == request.LegalRegisterDto.Email, cancellationToken);
            if (existingUser != null)
            {
                throw DomainException.Conflict("Пользователь с таким email уже существует.");
            }

            var subscriptionPrice = await _subscriptionPriceProvider.GetPriceByIdAsync(request.LegalRegisterDto.SubscriptionPriceId, cancellationToken);
            if (subscriptionPrice.Category != SubscriptionPriceCategory.Base)
            {
                throw DomainException.BadRequest("Некорректный идентификатор базовой подписки.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.LegalRegisterDto.Email,
                Email = request.LegalRegisterDto.Email,
                PasswordHash = string.Empty, // будет установлен позже
                CreatedAt = DateTime.UtcNow,
                IsActive = false,
                CustomerType = CustomerTypeEnum.Legal.ToString().ToLower(),
                SubscriptionType = subscriptionPrice.Code,
                CountRequestAI = subscriptionPrice.RequestCount
            };

            var role = await _dataContext.Roles.FirstOrDefaultAsync(r => r.Name == "free", cancellationToken);
            if (role == null)
            {
                throw new ArgumentException("Роль не найдена");
            }

            user.PasswordHash = _passwordHasher.HashPassword(request.LegalRegisterDto.Password);
            user.UserRoles.Add(new UserRole { Role = role });

            _dataContext.Users.Add(user);

            var consent = new UserConsent
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                DocumentVersion = request.LegalRegisterDto.OfferVersion,
                DocumentUrl = request.LegalRegisterDto.OfferLink,
                AcceptedAt = DateTime.UtcNow,
                IpAddress = request.IpAddress,
                IsAccepted = request.LegalRegisterDto.IsAccepted
            };

            _dataContext.UserConsents.Add(consent);

            var registration = new LegalRegistration
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                OrganizationName = request.LegalRegisterDto.OrganizationName,
                INN = request.LegalRegisterDto.INN,
                KPP = request.LegalRegisterDto.KPP,
                OGRN = request.LegalRegisterDto.OGRN,
                Address = request.LegalRegisterDto.Address,
                Phone = request.LegalRegisterDto.Phone,
                CreatedAt = DateTime.UtcNow
            };

            _dataContext.LegalRegistrations.Add(registration);

            string submissionNumber = GenerateSubmissionNumber();

            var legalSubmis = new LegalSubmission
            {
                Id = Guid.NewGuid(),
                SubmissionNumber = submissionNumber,
                TypeSubmission = "register",
                LegalRegistrationId = registration.Id
            };

            _dataContext.LegalSubmissions.Add(legalSubmis);

            await _dataContext.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new LegalUserRegisteredEvent(
                request.LegalRegisterDto.Email,
                request.LegalRegisterDto.OrganizationName,
                request.LegalRegisterDto.INN,
                request.LegalRegisterDto.KPP,
                request.LegalRegisterDto.OGRN,
                request.LegalRegisterDto.Address,
                request.LegalRegisterDto.Phone,
                submissionNumber,
                subscriptionPrice.Description,
                subscriptionPrice.DurationInMonths,
                subscriptionPrice.RequestCount,
                registration.CreatedAt), cancellationToken);

            return Unit.Value;
        }

        private string GenerateSubmissionNumber()
        {
            return $"REQ-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(10000, 99999)}";
        }
    }
}
