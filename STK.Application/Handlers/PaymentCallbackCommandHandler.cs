using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
using STK.Domain.Entities;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class PaymentCallbackCommandHandler : IRequestHandler<PaymentCallbackCommand, Unit>
    {
        private readonly DataContext _dataContext;

        public PaymentCallbackCommandHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<Unit> Handle(PaymentCallbackCommand request, CancellationToken cancellationToken)
        {
            var payReq = await _dataContext.PaymentRequests.FindAsync(request.OrderId);
            var timeUpdate = DateTime.UtcNow;

            if (payReq == null)
                throw new Exception("Payment request not found");

            if (request.Success && request.Status == "CONFIRMED" && !payReq.IsPaid)
            {
                payReq.IsPaid = true;
                var user = await _dataContext.Users
                    .Include(u => u.UserRoles) // Подгружаем роли пользователя
                    .FirstOrDefaultAsync(u => u.Id == payReq.UserId, cancellationToken);

                if (user == null)
                    throw new Exception("User not found");

                user.UpdatedAt = timeUpdate;
                user.IsActive = true;

                // Проверяем, есть ли уже у пользователя роль "user"
                var userRoleExists = user.UserRoles.Any(ur => ur.Role.Name == "user");

                if (!userRoleExists)
                {
                    // Находим роль "user" в базе
                    var userRole = await _dataContext.Roles
                        .FirstOrDefaultAsync(r => r.Name == "user", cancellationToken);

                    if (userRole == null)
                        throw new Exception("Role 'user' not found in database");

                    // Добавляем связь пользователя с ролью
                    user.UserRoles.Add(new UserRole
                    {
                        RoleId = userRole.Id
                    });
                }

                await _dataContext.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }

}
