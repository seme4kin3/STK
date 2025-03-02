using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
using STK.Application.DTOs.AuthDto;
using STK.Application.Services;
using STK.Domain.Entities;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthUserResponse>
    {
        private readonly DataContext _dataContext;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserCommandHandler(DataContext dataContext, IPasswordHasher passwordHasher)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username == request.Register.UserName);
            if (existingUser != null)
            {
                throw new Exception("Такой пользователь уже существует.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Register.UserName,
                PasswordHash = _passwordHasher.HashPassword(request.Register.Password),
                Email = request.Register.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var role = await _dataContext.Roles.FirstOrDefaultAsync(r => r.Name == request.Register.RoleName);
            if (role == null)
            {
                role = new Role { Id = Guid.NewGuid(), Name = request.Register.RoleName};
                _dataContext.Roles.Add(role);
            }

            user.UserRoles.Add(new UserRole { Role =  role });
            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return new AuthUserResponse { Success = true, Message = "Пользователь успешно зарегистрирован." };
        }
    }
}

