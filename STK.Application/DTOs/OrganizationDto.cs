using STK.Application.DTOs.SearchOrganizations;
using STK.Domain.Entities;
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
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string StatusOrg { get; set; }
        public RequisiteDto Requisites { get; set; } = new RequisiteDto();
        public List<ManagementDto> Managements { get; set; } = new List<ManagementDto>();
        public List<SearchEconomicActivityDto> EconomicActivities { get; set; } = new List<SearchEconomicActivityDto>();
        public List<CertificateDto> Certificate { get; set; } = new List<CertificateDto>();
        public List<BalanceSheetDto> BalanceSheets { get; set; } = new List<BalanceSheetDto>();
        public List<FinancialResultDto> FinancialResults { get; set; } = new List<FinancialResultDto>();
        public List<LicenseDto> Licenses { get; set; } = new List<LicenseDto>();
        public List<TaxModeDto> TaxModes { get; set; } = new List<TaxModeDto>();

    }
}
