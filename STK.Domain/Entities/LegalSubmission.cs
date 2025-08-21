
namespace STK.Domain.Entities
{
    public class LegalSubmission
    {
        public Guid Id { get; set; }
        public string SubmissionNumber { get; set; }
        public string TypeSubmission {  get; set; }
        public DateTime DateOfCreate { get; set; } = DateTime.UtcNow;
        public Guid LegalRegistrationId { get; set; }
        public LegalRegistration LegalRegistration { get; set; }

    }
}
