
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
        public string CaseNumber { get; set; }
        public Guid OrganizationId { get; set; }
    }

    public enum ArbitrationPartyRole
    {
        Any = 0,       // без фильтра
        Claimant = 1,  // истец
        Respondent = 2 // ответчик
    }
}