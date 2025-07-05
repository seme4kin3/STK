using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
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

        public RegisterUserCommandHandler(DataContext dataContext, IPasswordHasher passwordHasher)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {

            //if (!Enum.TryParse<SubscriptionType>(request.RegisterDto.SubscriptionType, ignoreCase: true, out var subscriptionType))
            //{
            //    return DomainException.ConflictSubscription("Некорректно задан тип подписки.").Message;
            //}

            var existingUser = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username == request.RegisterDto.Email);
            if (existingUser != null)
            {
                throw DomainException.Conflict("Пользователь с таким email уже существует.");
            }

            int initialRequestCount = GetInitialRequestCount(request.RegisterDto.SubscriptionType.ToLower());

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.RegisterDto.Email,
                PasswordHash = _passwordHasher.HashPassword(request.RegisterDto.Password),
                Email = request.RegisterDto.Email,
                CreatedAt = DateTime.UtcNow,
                //SubscriptionType = subscriptionType,
                SubscriptionType = request.RegisterDto.SubscriptionType.ToLower(),
                CountRequestAI = initialRequestCount
            };

            var role = await _dataContext.Roles.FirstOrDefaultAsync(r => r.Name == request.RegisterDto.RoleName);
            if (role == null)
            {
                role = new Role { Id = Guid.NewGuid(), Name = request.RegisterDto.RoleName };
                _dataContext.Roles.Add(role);
            }

            user.UserRoles.Add(new UserRole { Role =  role });
            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return user.Email;
        }

        private int GetInitialRequestCount(string subscriptionType)
        {
            return subscriptionType switch
            {
                "nosubscription" => 0,
                "free" => 0,
                "standard" => 3,
                "premium" => 15,
                _ => throw new ArgumentOutOfRangeException(nameof(subscriptionType), "Некорректно задан тип подписки.")
            };
        }
    }
}

