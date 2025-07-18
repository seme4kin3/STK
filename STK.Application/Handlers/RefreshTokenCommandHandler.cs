using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using STK.Application.Commands;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using STK.Application.Services;
using STK.Domain.Entities;
using STK.Persistance;
using System.Security.Claims;

namespace STK.Application.Handlers
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthTokenResponse>
    {
        private readonly IJwtService _jwtService;
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;

        public RefreshTokenCommandHandler(IJwtService jwtService, DataContext dataContext, IConfiguration configuration)
        {
            _jwtService = jwtService;
            _dataContext = dataContext;
            _configuration = configuration;
        }

        //public async Task<AuthTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        //{
        //    var principal = _jwtService.GetPrincipalFromExpiredToken(request.RefreshToken);
        //    var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        //    {
        //        throw new DomainException("Invalid token.", 401);
        //    }

        //    var user = await _dataContext.Users.Include(u => u.RefreshTokens)
        //        .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        //    if (user == null)
        //    {
        //        throw new DomainException("User not found.", 404);
        //    }

        //    var refreshToken = user.RefreshTokens
        //        .FirstOrDefault(rt => rt.Token == request.RefreshToken);

        //    _dataContext.RefreshTokens.RemoveRange(refreshToken);

        //    if (refreshToken == null || refreshToken.UserId != user.Id || refreshToken.IsExpired || refreshToken.Revoked != null)
        //    {
        //        throw new DomainException("Invalid refresh token.", 401);
        //    }

        //    var newAccessToken = _jwtService.GenerateAccessToken(user);
        //    var newRefreshToken = _jwtService.GenerateRefreshToken(user);

        //    refreshToken.Revoked = DateTime.UtcNow;
        //    user.RefreshTokens.Add(new RefreshToken
        //    {
        //        Token = newRefreshToken,
        //        Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"])),
        //        Created = DateTime.UtcNow,
        //        UserId = user.Id
        //    });

        //    await _dataContext.SaveChangesAsync(cancellationToken);

        //    return new AuthTokenResponse
        //    {
        //        AccessToken = newAccessToken,
        //        RefreshToken = newRefreshToken,
        //        UserInfo = new UserInfoDto
        //        {
        //            Email = user.Email,
        //            UserId = user.Id,
        //            Subscription = Enum.TryParse<SubscriptionType>(user.SubscriptionType, true, out var subscriptionType)
        //                ? subscriptionType
        //                : SubscriptionType.NoSubscription,
        //            CountRequest = user.CountRequestAI ?? 0
        //        }
        //    };
        //}

        public async Task<AuthTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _dataContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Валидация refresh token и извлечение пользователя
                var principal = _jwtService.GetPrincipalFromExpiredToken(request.RefreshToken);
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                {
                    throw DomainException.Unauthorized("Invalid token.");
                }

                // 2. Получение пользователя с refresh tokens
                var user = await _dataContext.Users
                    .Include(u => u.RefreshTokens)
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    throw DomainException.Unauthorized("User not found.");
                }

                // 3. Проверка активности пользователя
                if (!user.IsActive)
                {
                    throw DomainException.Forbidden("User account is inactive.");
                }

                // 4. Поиск текущего refresh token
                var currentRefreshToken = user.RefreshTokens
                    .FirstOrDefault(rt => rt.Token == request.RefreshToken && rt.IsActive);

                // 5. Валидация refresh token
                if (currentRefreshToken == null)
                {
                    throw DomainException.Unauthorized("Invalid refresh token.");
                }

                if (currentRefreshToken.IsExpired)
                {
                    throw DomainException.Unauthorized("Refresh token has expired.");
                }

                // 6. Удаление ВСЕХ старых refresh tokens
                var oldTokensCount = user.RefreshTokens.Count;
                _dataContext.RefreshTokens.RemoveRange(user.RefreshTokens);


                // 7. Генерация новых токенов
                var newAccessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken(user);

                // 8. Создание нового refresh token entity
                var newRefreshTokenEntity = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    Token = newRefreshToken,
                    Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"])),
                    Created = DateTime.UtcNow,
                    UserId = user.Id,

                };

                _dataContext.RefreshTokens.Add(newRefreshTokenEntity);

                // 9. Обновление информации о последнем обновлении токена


                await _dataContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new AuthTokenResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    UserInfo = new UserInfoDto
                    {
                        Email = user.Email,
                        UserId = user.Id,
                        Subscription = Enum.TryParse<SubscriptionType>(user.SubscriptionType, true, out var subscriptionType)
                            ? subscriptionType
                            : SubscriptionType.BaseQuarter,
                        CountRequest = user.CountRequestAI ?? 0
                    }
                };
            }
            catch (DomainException)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new DomainException("An error occurred during token refresh.", 500);
            }
        }
    }
}

