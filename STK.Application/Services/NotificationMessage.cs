
namespace STK.Application.Services
{
    public record NotificationMessage(
        Guid MessageId,
        string Title,
        string Content,
        DateTime CreatedAt,
        Guid? OrgId,
        string TableName,
        bool IsUrgent = false,
        string NotificationType = "regular");
}
