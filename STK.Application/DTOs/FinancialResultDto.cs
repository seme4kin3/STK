
namespace STK.Application.DTOs
{
    public class FinancialResultDto
    {
        public string Type { get; set; }
        public int Year { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? CostOfSales { get; set; }
        public decimal? GrossProfitRevenue { get; set; }
        public decimal? GrossProfitEarnings { get; set; }
        public decimal? SalesProfit { get; set; }
        public decimal? ProfitBeforeTax { get; set; }
        public decimal? NetProfit { get; set; }
        public decimal? IncomeTaxe { get; set; }
        public decimal? TaxFee { get; set; }

    }
}
