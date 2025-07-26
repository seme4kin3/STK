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

        public RegisterUserCommandHandler(DataContext dataContext, IPasswordHasher passwordHasher,
            IMediator mediator, TBankPaymentService payment)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
            _mediator = mediator;
            _payment = payment;
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
                    PasswordHash = string.Empty, // будет установлен позже
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

                var orderId = Guid.NewGuid().ToString();

                //Изменить URL для калбека от т -банка
                //var notificationUrl = "https://rail-stat.ru/api/payment-callback";

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
                SubscriptionType.BaseQuarter => 30000,
                SubscriptionType.BaseYear => 60000,
                _ => throw new ArgumentOutOfRangeException(nameof(subscriptionType), "Некорректно задан тип подписки.")
            };
        }
    }
}

