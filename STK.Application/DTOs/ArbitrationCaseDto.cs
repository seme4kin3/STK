
namespace STK.Application.DTOs
{
    public class ArbitrationCaseDto
    {
        public Guid Id { get; set; }
        public string Instance { get; set; }
        public DateTime DateOfCreateCase { get; set; }
        public string Claimant { get; set; }
        public string Respondent { get; set; }
        public string Url { get; set; }
        public string Judge { get; set; }
        public Guid OrganizationId { get; set; }
    }
}