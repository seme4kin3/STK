using MediatR;

namespace STK.Application.Commands
{
    public class RemoveFavoriteCertificateCommand : IRequest<Unit>
    {
        public Guid UserId { get; set; }
        public Guid CertificateId { get; set; }
    }
}
