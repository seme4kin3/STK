using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
using STK.Application.Middleware;
using STK.Application.Services;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, bool>
    {
        private readonly DataContext _dataContext;
        private readonly IPasswordHasher _passwordHasher;

        public ChangeUserPasswordCommandHandler(DataContext dataContext, IPasswordHasher passwordHasher)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<bool> Handle(ChangeUserPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users
                .FirstOrDefaultAsync(u => u.Email == command.BaseUserDto.Email, cancellationToken);

            if (user == null)
            {
                throw DomainException.UserNotFound("Пользователь с таким email не найден.");
            }

            var hashedPassword = _passwordHasher.HashPassword(command.BaseUserDto.Password);
            user.PasswordHash = hashedPassword;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
