using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
using STK.Domain.Entities;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class AddToFavoriteCertificateCommandHandler : IRequestHandler<AddFavoriteCertificateCommand, Unit>
    {
        private readonly DataContext _dataContext;
        public AddToFavoriteCertificateCommandHandler(DataContext dataContext) 
        {
            _dataContext = dataContext;
        }

        public async Task<Unit> Handle(AddFavoriteCertificateCommand request, CancellationToken cancellationToken)
        {
            var existingFavorite = await _dataContext.UsersFavoritesCertificates
                .FirstOrDefaultAsync(ufo => ufo.UserId == request.UserId && ufo.CertificateId == request.CertificateId, cancellationToken);

            if (existingFavorite != null)
            {
                throw new InvalidOperationException("Certificate is already in favorites.");
            }

            var favorite = new UserFavoriteCertificate
            {
                UserId = request.UserId,
                CertificateId = request.CertificateId,
                DateAddedOn = DateTime.UtcNow,
            };

            _dataContext.UsersFavoritesCertificates.Add(favorite);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
