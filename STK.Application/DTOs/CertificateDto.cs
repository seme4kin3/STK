using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs
{
    public class CertificateDto
    {
        public Guid Id { get; set; }
        public string NameOrganization { get; set; }
        public string Tittle { get; set; }
        public string CertificationObject { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public DateTime DateOfCertificateExpiration { get; set; }
        public DateTime DateOfIssueCertificate { get; set; }
        public string DeclarationOfConformity { get; set; }
        public string Status { get; set; }
    }
}
