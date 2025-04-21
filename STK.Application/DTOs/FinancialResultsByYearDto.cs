
namespace STK.Application.DTOs
{
    public class FinancialResultsByYearDto
    {
        public int Year { get; set; }
        public FinancialResultDto Profit { get; set; }
        public FinancialResultDto Revenue { get; set; }
    }
}
