
namespace STK.Domain.Entities
{
    public class Bankruptcy
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string StatusCase { get; set; }
        public string NumberCase { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
