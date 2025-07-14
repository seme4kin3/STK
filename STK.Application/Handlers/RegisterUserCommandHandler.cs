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

        public RegisterUserCommandHandler(DataContext dataContext, IPasswordHasher passwordHasher, IMediator mediator)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
            _mediator = mediator;
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

                int initialRequestCount = GetInitialRequestCount(request.RegisterDto.Subscription);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.RegisterDto.Email,
                    PasswordHash = _passwordHasher.HashPassword(request.RegisterDto.Password),
                    Email = request.RegisterDto.Email,
                    CreatedAt = DateTime.UtcNow,
                    SubscriptionType = request.RegisterDto.Subscription.ToString().ToLower(),
                    CountRequestAI = initialRequestCount,
                    CustomerType = request.RegisterDto.CustomerType.ToString().ToLower()
                };

                var role = await _dataContext.Roles.FirstOrDefaultAsync(r => r.Name == request.RegisterDto.Role.ToString(), cancellationToken);
                if (role == null)
                {
                    role = new Role { Id = Guid.NewGuid(), Name = request.RegisterDto.Role.ToString() };
                    _dataContext.Roles.Add(role);
                }

                if(request.RegisterDto.CustomerType == CustomerTypeEnum.Legal)
                {
                    await _mediator.Publish(new UserRegisteredEvent(
                        user.Email,
                        user.CreatedAt), cancellationToken);
                }

                user.UserRoles.Add(new UserRole { Role = role });
                _dataContext.Users.Add(user);
                await _dataContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return user.Email;
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
                SubscriptionType.NoSubscription => 0,
                SubscriptionType.Free => 0,
                SubscriptionType.Standard => 3,
                SubscriptionType.Premium => 15,
                _ => throw new ArgumentOutOfRangeException(nameof(subscriptionType), "Некорректно задан тип подписки.")
            };
        }
    }
}

