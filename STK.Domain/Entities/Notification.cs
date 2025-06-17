
namespace STK.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string TableName { get; set; }
        public Guid? RelatedOrganizationId { get; set; }
    }
}
