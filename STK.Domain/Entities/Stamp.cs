
namespace STK.Domain.Entities
{
    public class Stamp
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string StampNum { get; set; }
        public string StampStatus { get; set; }
        public string Contragent { get; set; }
        public string Place { get; set; }
        public string Status { get; set; }
        public DateTime Registration { get; set; }
        public DateTime Validity { get; set; }
        public string Usage { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
