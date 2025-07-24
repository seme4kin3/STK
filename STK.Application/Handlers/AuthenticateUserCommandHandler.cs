using MediatR;
using STK.Application.Commands;
using STK.Application.Services;
using STK.Persistance;
using Microsoft.EntityFrameworkCore;
using STK.Domain.Entities;
using Microsoft.Extensions.Configuration;
using STK.Application.Middleware;
using STK.Application.DTOs.AuthDto;

namespace STK.Application.Handlers
{
    public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, AuthTokenResponse>
    {
        private readonly DataContext _dataContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly LoginAttemptTracker _loginAttemptTracker;

        public AuthenticateUserCommandHandler(DataContext dataContext, IPasswordHasher passwordHasher,
            IJwtService jwtService, IConfiguration configuration, LoginAttemptTracker loginAttemptTracker)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _configuration = configuration;
            _loginAttemptTracker = loginAttemptTracker;
        }

        public async Task<AuthTokenResponse> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Проверка блокировки
                if (await _loginAttemptTracker.IsLockedOutAsync(request.AuthDto.Email))
                {
                    throw DomainException.TooManyAttempts("Аккаунт временно заблокирован.");
                }

                // 2. Получение пользователя с ролями
                var user = await _dataContext.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Username == request.AuthDto.Email, cancellationToken);

                // 3. Проверка учетных данных
                if (user == null || !_passwordHasher.VerifyPassword(request.AuthDto.Password, user.PasswordHash))
                {
                    _loginAttemptTracker.RecordFailedAttempt(request.AuthDto.Email);

                    // Добавить задержку для замедления атак
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                    throw DomainException.Unauthorized("Неверные учетные данные.");
                }

                //// 4. Проверка активности аккаунта
                //if (!user.IsActive)
                //{
                //    throw DomainException.Forbidden("Аккаунт неактивен.");
                //}

                // 6. Успешная аутентификация
                _loginAttemptTracker.ResetAttempts(request.AuthDto.Email);

                // 7. Генерация токенов
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken(user);

                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"])),
                    Created = DateTime.UtcNow,
                    UserId = user.Id
                };

                _dataContext.Add(refreshTokenEntity);
                await _dataContext.SaveChangesAsync(cancellationToken);

                return new AuthTokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    UserInfo = new UserInfoDto
                    {
                        Email = user.Email,
                        UserId = user.Id,
                        //Subscription = Enum.TryParse<SubscriptionType>(user.SubscriptionType, true, out var subscriptionType)
                        //      ? subscriptionType
                        //      : SubscriptionType.BaseQuarter,
                        SubscriptionType = user.SubscriptionType,
                        SubscriptionEndTime = user.SubscriptionEndTime,
                        CustomerType = user.CustomerType,
                        CountRequest = user.CountRequestAI ?? 0,
                        IsActive = user.IsActive,
                    }
                };
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DomainException("Произошла внутренняя ошибка.", 500, "INTERNAL_ERROR");
            }
        }
    }
}
