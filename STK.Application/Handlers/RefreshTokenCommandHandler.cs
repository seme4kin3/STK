using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using STK.Application.Commands;
//using STK.Application.DTOs.AuthDto;
using STK.Application.DTOs.AuthDtoTest;
using STK.Application.Middleware;
using STK.Application.Services;
using STK.Domain.Entities;
using STK.Persistance;
using System.Security.Claims;

namespace STK.Application.Handlers
{
    //public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthTokenResponse>
    //{
    //    private readonly IJwtService _jwtService;
    //    private readonly DataContext _dataContext;
    //    private readonly IConfiguration _configuration;

    //    public RefreshTokenCommandHandler(IJwtService jwtService, IConfiguration configuration, DataContext dataContext)
    //    {
    //        _jwtService = jwtService;
    //        _dataContext = dataContext;
    //        _configuration = configuration;
    //    }

    //    public async Task<AuthTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    //    {
    //        var principal = _jwtService.GetPrincipalFromExpiredToken(request.RefreshTokenRequest.RefreshToken);
    //        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    //        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
    //        {
    //            throw new Exception("Invalid token.");
    //        }

    //        var user = await _dataContext.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == userId);
    //        if (user == null)
    //        {
    //            throw new Exception("User not found.");
    //        }


    //        var refreshToken = user.RefreshTokens
    //            .FirstOrDefault(rt => rt.Token == request.RefreshTokenRequest.RefreshToken);

    //        if (refreshToken == null || refreshToken.UserId != user.Id || refreshToken.IsExpired || refreshToken.Revoked != null)
    //        {
    //            throw new Exception("Invalid refresh token.");
    //        }

    //        var newAccessToken = _jwtService.GenerateAccessToken(user);
    //        var newRefreshToken = _jwtService.GenerateRefreshToken(user);

    //        refreshToken.Revoked = DateTime.UtcNow;
    //        user.RefreshTokens.Add(new RefreshToken
    //        {
    //            Token = newRefreshToken,
    //            Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"])),
    //            Created = DateTime.UtcNow
    //        });

    //        await _dataContext.SaveChangesAsync(cancellationToken);

    //        return new AuthTokenResponse
    //        {
    //            AccessToken = newAccessToken,
    //            RefreshToken = newRefreshToken
    //        };
    //    }
    //}

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

        public async Task<AuthTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.RefreshTokenRequest.RefreshToken);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new DomainException("Invalid token.", 401);
            }

            var user = await _dataContext.Users.Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
            {
                throw new DomainException("User not found.", 404);
            }

            var refreshToken = user.RefreshTokens
                .FirstOrDefault(rt => rt.Token == request.RefreshTokenRequest.RefreshToken);

            if (refreshToken == null || refreshToken.UserId != user.Id || refreshToken.IsExpired || refreshToken.Revoked != null)
            {
                throw new DomainException("Invalid refresh token.", 401);
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken(user);

            refreshToken.Revoked = DateTime.UtcNow;
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"])),
                Created = DateTime.UtcNow,
                UserId = user.Id
            });

            await _dataContext.SaveChangesAsync(cancellationToken);

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
                        : SubscriptionType.NoSubscription,
                    CountRequest = user.CountRequestAI ?? 0
                }
            };
        }
    }
}

