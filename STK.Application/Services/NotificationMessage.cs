

namespace STK.Application.Services
{
    public record NotificationMessage(
        string Title,
        string Content,
        DateTime CreatedAt,
        bool IsUrgent = false,
        string NotificationType = "regular");
}
