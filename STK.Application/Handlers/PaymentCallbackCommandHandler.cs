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
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == payReq.UserId, cancellationToken);

                if (user == null)
                    throw new Exception("User not found");

                user.UpdatedAt = timeUpdate;
                user.IsActive = true;

                var targetRole = await _dataContext.Roles
                    .FirstOrDefaultAsync(r => r.Name == "user", cancellationToken);

                if (targetRole == null)
                    throw new Exception("Role 'user' not found in database");

                // Удаляем существующие роли пользователя
                foreach (var ur in user.UserRoles.ToList())
                {
                    _dataContext.UserRoles.Remove(ur);
                }

                // Добавляем новую связь с ролью 'user'
                _dataContext.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = targetRole.Id
                });

                await _dataContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new Exception("Payment request with not status CONFIRMED");
            }

            return Unit.Value;
        }
    }

}
