
namespace STK.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string TableName { get; set; }
        public Guid RecordId { get; set; }
        public Guid? RelatedOrganizationId { get; set; }
        public string Operation { get; set; }
        public string? OldData { get; set; } // JSONB хранится как строка
        public string? NewData { get; set; } // JSONB хранится как строка
        public DateTime ChangedAt { get; set; }
    }
}
