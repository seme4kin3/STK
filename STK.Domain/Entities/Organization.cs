
namespace STK.Domain.Entities
{
    public class Organization 
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public bool? Address { get; set; }
        public string IndexAddress { get; set; }
        public Guid? ParentOrganizationId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string StatusOrg { get; set; }
        public DateTime? CreatedAtDate { get; set; }
        public DateTime? LastChangedAtDate { get; set; }
        public DateTime? AddressAdded { get; set; }
        public Requisite Requisites { get; set; }
        public ICollection<Management> Managements { get; set; } = new List<Management>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public ICollection<BalanceSheet> BalanceSheets { get; set; } = new List<BalanceSheet>();
        public ICollection<FinancialResult> FinancialResults { get; set; } = new List<FinancialResult>();
        public ICollection<License> Licenses { get; set; } = new List<License>();
        public ICollection<TaxMode> TaxesModes { get; set; } = new List<TaxMode>();
        public ICollection<Stamp> Stamps { get; set; } = new List<Stamp>();
        public ICollection<Bankruptcy> Bankruptcies { get; set; } = new List<Bankruptcy>();
        public ICollection<ArbitrationCase> ArbitrationsCases { get; set; } = new List<ArbitrationCase>();
        public ICollection<OrganizationEconomicActivity> OrganizationsEconomicActivities { get; set; } = new List<OrganizationEconomicActivity>();
        public ICollection<UserFavoriteOrganization> FavoritedByUsers { get; set;} = new List<UserFavoriteOrganization>();
        public ICollection<UserCreatedOrganization> UserCreatedOrganizations { get; set; } = new List<UserCreatedOrganization>();
        public ICollection<BankruptcyIntention> BankruptcyIntentions { get; set; } = new List<BankruptcyIntention>();
        public ICollection<TaxArrears> TaxesArrears { get; set; } = new HashSet<TaxArrears>();
    }
}
