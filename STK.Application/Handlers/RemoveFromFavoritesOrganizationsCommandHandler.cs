using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
using STK.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.Handlers
{
    public class RemoveFromFavoritesOrganizationsCommandHandler : IRequestHandler<FavoriteOrganizationCommand, Unit>
    {
        private readonly DataContext _dataContext;

        public RemoveFromFavoritesOrganizationsCommandHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<Unit> Handle(FavoriteOrganizationCommand request, CancellationToken cancellationToken)
        {
            var favorite = await _dataContext.UsersFavoritesOrganizations
                .FirstOrDefaultAsync(ufo => ufo.UserId == request.UserId && ufo.OrganizationId == request.OrganizationId, cancellationToken);

            if (favorite == null)
            {
                throw new InvalidOperationException("Organization is not in favorites.");
            }

            _dataContext.UsersFavoritesOrganizations.Remove(favorite);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
