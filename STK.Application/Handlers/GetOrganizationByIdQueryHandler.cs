using MediatR;
using STK.Application.DTOs;
using STK.Application.Queries;
using STK.Persistance.Interfaces;


namespace STK.Application.Handlers
{
    public class GetOrganizationByIdQueryHandler: IRequestHandler<GetOrganizationByIdQuery, OrganizationDto>
    {
        private readonly IOrganizationRepository _repository;

        public GetOrganizationByIdQueryHandler(IOrganizationRepository repository)
        {
            _repository = repository;
        }

        public async Task<OrganizationDto> Handle(GetOrganizationByIdQuery query, CancellationToken cancellationToken)
        {
            var organization = await _repository.GetOrganizationById(query.Id);
            if (organization == null) { return null; }

            var response = new OrganizationDto
            {
                Id = organization.Id,
                Name = organization.Name,
                FullName = organization.FullName,
                Adress = $"{organization.Adress} {organization.IndexAdress}",
                Requisites = new RequisiteDto
                {
                    INN = organization.Requisites.INN,
                    KPP = organization.Requisites.KPP,
                    OGRN = organization.Requisites.OGRN,
                    DateCreation = organization.Requisites.DateCreation,
                    EstablishmentCreateName = organization.Requisites.EstablishmentCreateName,
                    AuthorizedCapital = organization.Requisites.AuthorizedCapital,
                },
                Management = organization.Managements.Select(m => new ManagementDto
                {
                    FullName = $"{m.FirstName} {m.LastName}",
                    Position = m.Position,
                    INN = m.INN

                }).ToList(),
                EconomicActivities = organization.EconomicActivities.Select(e => new EconomicActivityDto
                {
                    OKVDNnumber = e.OKVDNnumber,
                    Discription = e.Discription,
                }).ToList(),
                Certificate = organization.Certificates.Select(c => new CertificateDto
                {
                    NameOrganization = c.NameOrganization,
                    Tittle = c.Tittle,
                    CertificationObject = c.CertificationObject,
                    City = c.City,
                    Country = c.Country,
                    DateOfCertificateExpiration = c.DateOfCertificateExpiration,
                    DateOfIssueCertificate = c.DateOfIssueCertificate,
                    DeclarationOfConformity = c.DeclarationOfConformity,
                    Status = c.Status,
                }).ToList()
            };

            return response;
        }


    }
}
