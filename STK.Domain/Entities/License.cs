
namespace STK.Domain.Entities
{
    public class License
    {
        public Guid Id { get; set; }
        public string NameTypeActivity { get; set; }
        public string SeriesNumber { get; set; }
        public DateTime DateOfIssue { get; set; }
        public string NameOrganizationIssued { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
