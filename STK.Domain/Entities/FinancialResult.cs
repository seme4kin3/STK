using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STK.Domain.Entities
{
    public class FinancialResult
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public int Year { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? CostOfSales { get; set; }
        public decimal? GrossProfitRevenue { get; set; }
        public decimal? GrossProfitEarnings { get; set; }
        public decimal? SalesProfit { get; set; }
        public decimal? ProfitBeforeTax { get; set; }
        public decimal? NetProfit { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
