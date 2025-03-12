using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
using STK.Domain.Entities;
using STK.Persistance;


namespace STK.Application.Handlers
{
    public class AddToFavoriteOrganizationCommandHandler : IRequestHandler<FavoriteOrganizationCommand, Unit>
    {
        private readonly DataContext _dataContext;
        
        public AddToFavoriteOrganizationCommandHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<Unit> Handle(FavoriteOrganizationCommand request, CancellationToken cancellationToken)
        {
            var existingFavorite = await _dataContext.UsersFavoritesOrganizations
                .FirstOrDefaultAsync(ufo => ufo.UserId == request.UserId && ufo.OrganizationId == request.OrganizationId, cancellationToken);

            if (existingFavorite != null)
            {
                throw new InvalidOperationException("Organization is already in favorites.");
            }

            var favorite = new UserFavoriteOrganization
            {
                UserId = request.UserId,
                OrganizationId = request.OrganizationId,
                DateAddedOn =  DateTime.UtcNow,
            };

            _dataContext.UsersFavoritesOrganizations.Add(favorite);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
