using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Application.DTOs
{
    public class OrganizationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Adress {  get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public RequisiteDto Requisites { get; set; }
        public List<ManagementDto> Management { get; set; } = new List<ManagementDto>();
        public List<EconomicActivityDto> EconomicActivities { get; set; } = new List<EconomicActivityDto> { };
        public List<CertificateDto> Certificate { get; set; } = new List<CertificateDto> { };
    }
}
