
namespace STK.Domain.Entities
{
    public class ArbitrationCase
    {
        public Guid Id { get; set; }
        public string Instance { get; set; }
        public DateTime DateOfCreateCase { get; set; }
        public string Claimant { get; set; }
        public string Respondent { get; set; }
        public string Url { get; set; }
        public string Judge { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
