using STK.Application.DTOs.SearchOrganizations;

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
        public bool IsFavorite { get; set; }
        public bool AddressBool { get; set; }
        public string TaxMode { get; set; }
        public RequisiteDto Requisites { get; set; } = new RequisiteDto();
        public List<ManagementDto> Managements { get; set; } = new List<ManagementDto>();
        public List<SearchEconomicActivityDto> EconomicActivities { get; set; } = new List<SearchEconomicActivityDto>();
        public List<CertificateDto> Certificates { get; set; } = new List<CertificateDto>();
        public List<BalanceSheetDto> BalanceSheets { get; set; } = new List<BalanceSheetDto>();
        public List<FinancialResultsByYearDto> FinancialResultsByYear { get; set; } = new List<FinancialResultsByYearDto>();
        public List<LicenseDto> Licenses { get; set; } = new List<LicenseDto>();
        public List<StampDto> Stamps { get; set; } = new List<StampDto>();
        public List<BankruptcyDto> Bankruptcies { get; set; } = new List<BankruptcyDto> ();
        public List<BankruptcyIntentionDto> BankruptcyIntentions { get; set; } = new List<BankruptcyIntentionDto>();
        public List<TaxArrearsDto> TaxArrears { get; set; } = new List<TaxArrearsDto> ();

    }
}
