
using System.Text.Json;

namespace STK.Application.DTOs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string TableName { get; set; }
        public Guid RecordId { get; set; }
        public Guid? RelatedOrganizationId { get; set; }
        public string Operation { get; set; }
        //public object? OldData { get; set; } // Десериализованный JSON
        //public object? NewData { get; set; } // Десериализованный JSON
        public JsonElement? OldData { get; set; }
        public JsonElement? NewData { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
