using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using STK.Application.Commands;
using STK.Application.DTOs;
using STK.Application.Services;
using STK.Domain.Entities;
using STK.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Handlers
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthTokenDto>
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

        public async Task<AuthTokenDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.RefreshTokenRequest.AccessToken);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new Exception("Invalid user ID in token.");
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

            await _dataContext.SaveChangesAsync();

            return new AuthTokenDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}

