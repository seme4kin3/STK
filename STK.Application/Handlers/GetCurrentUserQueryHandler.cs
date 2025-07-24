using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.DTOs.AuthDto;
using STK.Application.Middleware;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class GetCurrentUserQuery : IRequest<UserInfoDto>
    {
        public Guid UserId { get; set; }

        public GetCurrentUserQuery(Guid userId)
        {
            UserId = userId;
        }
    }
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserInfoDto>
    {
        private readonly DataContext _dataContext;

        public GetCurrentUserQueryHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<UserInfoDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new DomainException("User not found.", 404);
            }

            return new UserInfoDto
            {
                Email = user.Email,
                UserId = user.Id,
                //Subscription = Enum.TryParse<SubscriptionType>(user.SubscriptionType, true, out var subscriptionType)
                //      ? subscriptionType
                //      : SubscriptionType.BaseQuarter,
                SubscriptionType = user.SubscriptionType,
                SubscriptionEndTime = user.SubscriptionEndTime,
                CustomerType = user.CustomerType,
                CountRequest = user.CountRequestAI ?? 0,
                IsActive = user.IsActive
            };
        }
    }
}
