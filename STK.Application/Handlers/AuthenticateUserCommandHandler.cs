using MediatR;
using STK.Application.Commands;
using STK.Application.Services;
using STK.Persistance;
using Microsoft.EntityFrameworkCore;
using STK.Domain.Entities;
using Microsoft.Extensions.Configuration;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;

namespace STK.Application.Handlers
{
    public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, AuthTokenResponse>
    {
        private readonly DataContext _dataContext;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        
        public AuthenticateUserCommandHandler(DataContext dataContext, IPasswordHasher passwordHasher, IJwtService jwtService, IConfiguration configuration)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _configuration = configuration;
        }
        public async Task<AuthTokenResponse> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username == request.AuthDto.Email, cancellationToken);

            if (user == null || !_passwordHasher.VerifyPassword(request.AuthDto.Password, user.PasswordHash))
            {
                throw DomainException.Unauthorized("Неверный пароль или электронная почта.");
            }

            if (!user.IsActive)
            {
                throw DomainException.Forbidden($"У пользователя закончилась подписка.");
            }

            var token = _jwtService.GenerateAccessToken(user);
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
                AccessToken = token,
                RefreshToken = refreshToken,
                UserName = user.Username,
                UserId = user.Id,
                UserTypeSubscription =  user.SubscriptionType ?? "NoSubscription",
                //UserTypeSubscription = Enum.GetName(typeof(SubscriptionType),user.SubscriptionType) ?? ,
                CountRequest = user.CountRequestAI ?? 0
            };
        }
    }
}
