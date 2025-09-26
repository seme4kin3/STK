
namespace STK.Application.DTOs
{
    public class BankruptcyIntentionDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; }
        public DateTime? DatePublish { get; set; }
        public string TypeName { get; set; }
        public string DebtorName { get; set; }
        public string DebtorInn { get; set; }
        public string DebtorOgrn { get; set; }
        public string PublisherType { get; set; }
        public string PublisherFio { get; set; }
        public string PublisherInn { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
