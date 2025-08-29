using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class LogoutUserCommandHandler : IRequestHandler<LogoutCommand, string>
    {
        private readonly DataContext _dataContext;

        public LogoutUserCommandHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<string> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users
                        .Include(u => u.RefreshTokens) // Загружаем refresh токены
                        .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw DomainException.UserNotFound("Пользователь не найден.");
            }

            if(!string.IsNullOrEmpty(request.RefreshToken) &&
                user.CustomerType.Equals(CustomerTypeEnum.Legal.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var token = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken && rt.IsActive);
                if (token != null)
                {
                    token.Revoked = DateTime.UtcNow;
                }
            }
            else
            {
                foreach(var refreshToken in user.RefreshTokens.Where(rt => rt.IsActive))
                {
                    refreshToken.Revoked = DateTime.UtcNow;
                }
            }

            // Сохраняем изменения в базе данных
            await _dataContext.SaveChangesAsync(cancellationToken);

            return "Успешный выход";
        }
    }
}
