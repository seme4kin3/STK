using MediatR;
using Microsoft.EntityFrameworkCore;
using STK.Application.Commands;
using STK.Persistance;

namespace STK.Application.Handlers
{
    public class RemoveFavoriteCertificateCommandHandler : IRequestHandler<RemoveFavoriteCertificateCommand, Unit>
    {
        private readonly DataContext _dataContext;
        public RemoveFavoriteCertificateCommandHandler(DataContext dataContext) 
        { 
            _dataContext = dataContext;
        }

        public async Task<Unit> Handle(RemoveFavoriteCertificateCommand request, CancellationToken cancellationToken)
        {
            var favorite = await _dataContext.UsersFavoritesCertificates
                .FirstOrDefaultAsync(ufc => ufc.UserId == request.UserId && ufc.CertificateId == request.CertificateId, cancellationToken);

            if (favorite == null)
            {
                throw new InvalidOperationException("Certificate is not in favorites.");
            }

            _dataContext.UsersFavoritesCertificates.Remove(favorite);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
