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
        private readonly TBankPaymentService _payment;
        private readonly IEmailService _emailService;
        private readonly ISubscriptionPriceProvider _subscriptionPriceProvider;

        public RegisterUserCommandHandler(DataContext dataContext, IPasswordHasher passwordHasher,
            TBankPaymentService payment, IEmailService emailService, ISubscriptionPriceProvider subscriptionPriceProvider)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
            _payment = payment;
            _emailService = emailService;
            _subscriptionPriceProvider = subscriptionPriceProvider;
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

                var subscriptionPrice = await _subscriptionPriceProvider.GetBasePriceAsync(request.RegisterDto.SubscriptionType, cancellationToken);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.RegisterDto.Email,
                    PasswordHash = string.Empty, 
                    Email = request.RegisterDto.Email,
                    CreatedAt = DateTime.UtcNow,
                    SubscriptionType = subscriptionPrice.Code,
                    CustomerType = CustomerTypeEnum.Individual.ToString().ToLower(),
                    CountRequestAI = subscriptionPrice.RequestCount,
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

                var payment = await _payment.InitPaymentAsync(orderId, subscriptionPrice.Price, subscriptionPrice.Description, user.Email);

                var payRequest = new PaymentRequest
                {
                    Id = Guid.Parse(orderId),
                    UserId = user.Id,
                    Amount = subscriptionPrice.Price,
                    PaymentUrl = payment.PaymentURL,
                    CreatedAt = DateTime.UtcNow,
                    PaymentId = payment.PaymentId,
                    Description = subscriptionPrice.Description,
                    SubscriptionPriceId = subscriptionPrice.Id
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

    }
}

