
namespace STK.Domain.Entities
{
    public class BalanceSheet
    {
        public Guid Id { get; set; }
        public int Year { get; set; }
        public string AssetType { get; set; }
        public decimal? NonCurrentActive { get; set; }
        public decimal? CurrentActive { get; set; }
        public decimal? CapitalReserves { get; set; }
        public decimal? LongTermLiabilities { get; set; }
        public decimal? ShortTermLiabilities { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

    }
}
