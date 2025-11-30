
namespace STK.Application.DTOs.SearchOrganizations
{
    public class SearchCertificatesDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Applicant { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string CertificationObject { get; set; }
        public DateTime DateOfIssueCertificate { get; set; }
        public DateTime? DateOfCertificateExpiration { get; set; }
        public string CertificationType { get; set; }
        public string Status { get; set; }
        public string Manufacturer { get; set; }
        public string ManufacturerCountry { get; set; }
        public bool IsFavorite { get; set; }
        public string StatusChange { get; set; }
        public Guid OrganizationId { get; set; }
 
    }
}
