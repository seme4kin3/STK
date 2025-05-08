
namespace STK.Domain.Entities
{
    public class ArbitrationCase
    {
        public Guid Id { get; set; }
        public string Role { get; set; }
        public string Type { get; set; }
        public DateTime DateOfCreateCase { get; set; }  
        public string Court { get; set; }
        public string Magistrate { get; set; }
        public string Url { get; set; }
        public decimal AmountOfClaim { get; set; }
        public string Status { get; set; }
        public DateTime DateOfStatus { get; set; }
        public string Authority { get; set; }
        public string CourtJudgment { get; set; }
        public string CaseRegisterNumber { get; set; }
        public string TypeOfJudicialAct { get; set; }
        public string NameOfJudicialAct { get; set; }
        public string UrlOfJudicialAct { get; set; }
        public string AdditionalInformation { get; set; }
        public DateTime DateOfJudicialAct { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
