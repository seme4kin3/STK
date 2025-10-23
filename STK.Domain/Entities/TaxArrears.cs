
namespace STK.Domain.Entities
{
    public class TaxArrears
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateAdded { get; set; }
        public string Description { get; set; }
        public bool IsPayOff { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
