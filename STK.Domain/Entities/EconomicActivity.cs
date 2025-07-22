
namespace STK.Domain.Entities
{
    public class EconomicActivity
    {
        public Guid Id { get; set; }
        public string OKVDNumber { get; set; }
        public string Description {  get; set; }
        public ICollection<OrganizationEconomicActivity> OrganizationsEconomicActivities { get; set; } = new List<OrganizationEconomicActivity>();

    }
}
