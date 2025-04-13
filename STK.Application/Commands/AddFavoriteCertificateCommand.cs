using MediatR;


namespace STK.Application.Commands
{
    public class AddFavoriteCertificateCommand : IRequest<Unit>
    {
        public Guid UserId { get; set; }
        public Guid CertificateId { get; set; }
    }
}
