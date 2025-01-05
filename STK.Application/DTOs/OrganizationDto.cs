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
        public RequisiteDto Requisites { get; set; }
        public List<ManagementDto> Management { get; set; }
        public List<EconomicActivityDto> EconomicActivities { get; set; }
        public List<CertificateDto> Certificate { get; set; }
    }
}
