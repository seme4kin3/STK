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
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, string>
    {
        private readonly DataContext _dataContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMediator _mediator;
        private readonly TBankPaymentService _payment;
        private readonly IEmailService _emailService;

        public RegisterUserCommandHandler(DataContext dataContext, IPasswordHasher passwordHasher,
            IMediator mediator, TBankPaymentService payment, IEmailService emailService)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
            _mediator = mediator;
            _payment = payment;
            _emailService = emailService;
        }

        public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _dataContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var existingUser = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username == request.RegisterDto.Email, cancellationToken);
                if (existingUser != null)
                {
                    throw DomainException.Conflict("Пользователь с таким email уже существует.");
                }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.RegisterDto.Email,
                    PasswordHash = string.Empty, 
                    Email = request.RegisterDto.Email,
                    CreatedAt = DateTime.UtcNow,
                    SubscriptionType = request.RegisterDto.SubscriptionType.ToString().ToLower(),
                    CustomerType = CustomerTypeEnum.Individual.ToString().ToLower(),
                    CountRequestAI = 3,
                    IsActive = false
                };

                var role = await _dataContext.Roles.FirstOrDefaultAsync(r => r.Name == "free", cancellationToken);
                if (role == null)
                {
                    throw new ArgumentException("Роль не найдена");
                }

                user.UserRoles.Add(new UserRole { Role = role });
                user.PasswordHash = _passwordHasher.HashPassword(request.RegisterDto.Password);
                _dataContext.Users.Add(user);

                var consent = new UserConsent
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    DocumentVersion = request.RegisterDto.OfferVersion,
                    DocumentUrl = request.RegisterDto.OfferLink,
                    AcceptedAt = DateTime.UtcNow,
                    IpAddress = request.IpAddress,
                    IsAccepted = request.RegisterDto.IsAccepted
                };

                _dataContext.UserConsents.Add(consent);

                var orderId = Guid.NewGuid().ToString();

                var amount = GetInitialRequestCount(request.RegisterDto.SubscriptionType);

                var payment = await _payment.InitPaymentAsync(orderId, amount, "Доступ к сервису", user.Email);

                var payRequest = new PaymentRequest
                {
                    Id = Guid.Parse(orderId),
                    UserId = user.Id,
                    Amount = amount,
                    PaymentUrl = payment.PaymentURL,
                    CreatedAt = DateTime.UtcNow,
                    PaymentId = payment.PaymentId,
                    Description = "Доступ к сервису"
                };
                _dataContext.PaymentRequests.Add(payRequest);

                await _dataContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                try
                {
                    var emailContent = new EmailContent
                    {
                        To = user.Email,
                        Subject = "Успешная регистрация в системе «РейлСтат»",
                        Body = $"Вы успешно зарегистрировались в системе «РейлСтат». Ваш пароль: {request.RegisterDto.Password}",
                        IsHtml = false
                    };

                    await _emailService.SendEmailAsync(emailContent);
                }
                catch
                {
                    
                }

                return payment.PaymentURL;
            }

            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private int GetInitialRequestCount(SubscriptionType subscriptionType)
        {
            return subscriptionType switch
            {
                SubscriptionType.BaseQuarter => 60000,
                SubscriptionType.BaseYear => 120000,
                _ => throw new ArgumentOutOfRangeException(nameof(subscriptionType), "Некорректно задан тип подписки.")
            };
        }
    }
}

