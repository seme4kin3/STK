using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using STK.Application.Handlers;

namespace STK.Application.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailContent emailContent);
    }
    public class EmailContent
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; } = false;
    }
    public class EmailService : IEmailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;

            _smtpClient = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = _emailSettings.EnableSsl
            };
        }

        public async Task SendEmailAsync(EmailContent emailContent)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = emailContent.Subject,
                    Body = emailContent.Body,
                    IsBodyHtml = emailContent.IsHtml
                };

                mailMessage.To.Add(emailContent.To);

                await _smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email успешно отправлен на {To}", emailContent.To);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке email на {To}", emailContent.To);
                throw;
            }
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}
