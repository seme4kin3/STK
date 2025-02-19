using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using STK.Application.Commands;
using STK.Application.DTOs;
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

        public RefreshTokenCommandHandler(IJwtService jwtService, IConfiguration configuration, DataContext dataContext)
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
                throw new Exception("Invalid token.");
            }

            var user = await _dataContext.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            
            var refreshToken = user.RefreshTokens
                .FirstOrDefault(rt => rt.Token == request.RefreshTokenRequest.RefreshToken);

            if (refreshToken == null || refreshToken.UserId != user.Id || refreshToken.IsExpired || refreshToken.Revoked != null)
            {
                throw new Exception("Invalid refresh token.");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken(user);

            refreshToken.Revoked = DateTime.UtcNow;
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"])),
                Created = DateTime.UtcNow
            });

            await _dataContext.SaveChangesAsync(cancellationToken);

            return new AuthTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}

