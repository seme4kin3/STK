using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs
{
    public class CertificateDto
    {
        public string Title { get; set; }
        public string Applicant { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string CertificationObject { get; set; }
        public DateTime DateOfIssueCertificate { get; set; }
        public DateTime? DateOfCertificateExpiration { get; set; }
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
    }
}
