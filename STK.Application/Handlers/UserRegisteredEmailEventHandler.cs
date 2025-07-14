
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using STK.Application.Services;

namespace STK.Application.Handlers
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string AdminEmail { get; set; }
    }
    public record UserRegisteredEvent(string Email, DateTime RegisteredAt) : INotification;
    public class UserRegisteredEmailEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<UserRegisteredEmailEventHandler> _logger;
        private readonly EmailSettings _emailSettings;

        public UserRegisteredEmailEventHandler(
            IEmailService emailService,
            ILogger<UserRegisteredEmailEventHandler> logger,
            IOptions<EmailSettings> emailSettings)
        {
            _emailService = emailService;
            _logger = logger;
            _emailSettings = emailSettings.Value;
        }

        public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var emailContent = new EmailContent
                {
                    To = _emailSettings.AdminEmail,
                    Subject = "Новая регистрация на сайте",
                    Body = $@"
                        <h2>Поступила заявка на регистрацию в системе от юридичекого лица</h2>
                        <p><strong>Email:</strong> {notification.Email}</p>
                        <p><strong>Время регистрации:</strong> {notification.RegisteredAt:dd.MM.yyyy HH:mm:ss}</p>
                    ",
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(emailContent);

                _logger.LogInformation("Уведомление о регистрации пользователя {Email} отправлено", notification.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке уведомления о регистрации пользователя {Email}", notification.Email);
            }
        }
    }
}
