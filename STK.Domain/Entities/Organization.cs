using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Domain.Entities
{
    public class Organization 
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string IndexAddress { get; set; }
        public Guid? ParentOrganizationId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string StatusOrg { get; set; }
        public Requisite Requisites { get; set; }
        public ICollection<Management> Managements { get; set; } = new List<Management>();
        public ICollection<EconomicActivity> EconomicActivities { get; set; } = new List<EconomicActivity>();
        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
        public ICollection<BalanceSheet> BalanceSheets { get; set; } = new List<BalanceSheet>();
        public ICollection<FinancialResult> FinancialResults { get; set; } = new List<FinancialResult>();
        public ICollection<License> Licenses { get; set; } = new List<License>();
        public ICollection<TaxMode> TaxesModes { get; set; } = new List<TaxMode>();
        public ICollection<OrganizationEconomicActivity> OrganizationsEconomicActivities { get; set; } = new List<OrganizationEconomicActivity>();

    }
}
