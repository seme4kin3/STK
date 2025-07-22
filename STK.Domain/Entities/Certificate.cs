

namespace STK.Domain.Entities
{
    public class Certificate
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Applicant { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string CertificationObject { get; set; }
        public DateTime DateOfIssueCertificate { get; set; }
        public DateTime? DateOfCertificateExpiration {  get; set; }
        public string CertificationType { get; set; }
        public string Status { get; set; }
        public DateTime? CertificateSuspensionDate { get; set; }
        public DateTime? CertificateRenewalDate { get; set; }
        public string PrescriptionReason { get; set; }
        public string SuspensionHistory { get; set; }
        public string Manufacturer { get; set; }
        public string ManufacturerCity { get; set; }
        public string ManufacturerAddress { get; set; }
        public string ManufacturerCountry { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public ICollection<UserFavoriteCertificate> FavoritedByUsers { get; set; }
    }
}
