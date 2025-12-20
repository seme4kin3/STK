using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using STK.Application.DTOs.AuthDto;
using STK.Application.Services;
using STK.Domain.Entities;

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
    public record LegalUserRegisteredEvent(
       string Email,
       string OrganizationName,
       string INN,
       string KPP,
       string OGRN,
       string Address,
       string Phone,
       string SubmissionNumber,
       string SubscriptionDescription,
       int? SubscriptionDurationInMonths,
       int? SubscriptionRequestCount,
       DateTime RegisteredAt) : INotification;

    public record LegalUserSubscriptionUpdatedEvent(
        string Email,
        string OrganizationName,
        string INN,
        string KPP,
        string OGRN,
        string Address,
        string Phone,
        string SubmissionNumber,
        SubscriptionPriceCategory Category,
        string SubscriptionDescription,
        int? SubscriptionDurationInMonths,
        int? SubscriptionRequestCount,
        DateTime RequestedAt) : INotification;
    public class LegalUserRegisteredEmailEventHandler : INotificationHandler<LegalUserRegisteredEvent>, INotificationHandler<LegalUserSubscriptionUpdatedEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<LegalUserRegisteredEmailEventHandler> _logger;
        private readonly EmailSettings _emailSettings;

        public LegalUserRegisteredEmailEventHandler(
            IEmailService emailService,
            ILogger<LegalUserRegisteredEmailEventHandler> logger,
            IOptions<EmailSettings> emailSettings)
        {
            _emailService = emailService;
            _logger = logger;
            _emailSettings = emailSettings.Value;
        }

        public async Task Handle(LegalUserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var timeSubscription = notification.SubscriptionDurationInMonths.HasValue
                    ? $"{notification.SubscriptionDurationInMonths.Value} мес."
                    : "неизвестный";

                var body = $@"<h2>Поступила заявка <strong>{notification.SubmissionNumber}</strong> на регистрацию от юридического лица</h2>
                                <p><strong>Организация:</strong> {notification.OrganizationName}</p>
                                <p><strong>ИНН:</strong> {notification.INN}</p>
                                <p><strong>КПП:</strong> {notification.KPP}</p>
                                <p><strong>ОГРН:</strong> {notification.OGRN}</p>
                                <p><strong>Email:</strong> {notification.Email}</p>
                                <p><strong>Телефон:</strong> {notification.Phone}</p>
                                <p><strong>Адрес:</strong> {notification.Address}</p>
                                <p><strong>Описание подписки:</strong> {notification.SubscriptionDescription}</p>
                                <p><strong>Срок доступа:</strong> {timeSubscription}</p>
                                <p><strong>Время регистрации:</strong> {notification.RegisteredAt:dd.MM.yyyy HH:mm:ss}</p>";

                var emailContent = new EmailContent
                {
                    To = _emailSettings.AdminEmail,
                    Subject = $"Заявка на регистрацию юридического лица.  Номер заявки: {notification.SubmissionNumber}",
                    Body = body,
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(emailContent);
                _logger.LogInformation("Email with legal registration info sent to admin");

                var userBody = @"
                    <p>Благодарим вас за регистрацию в системе «РейлСтат».</p>
<p>
                    <p>В настоящий момент для вашей организации формируется счёт на оплату доступа к системе.</p>
                    <p>Для корректного оформления счёта просим вас в ответном письме направить карточку предприятия (реквизиты вашей организации).</p>

                    <p>После получения реквизитов мы подготовим и направим вам счёт на указанную электронную почту.</p>

                    <p>Если у вас возникнут вопросы, пожалуйста, свяжитесь с нашей службой поддержки.</p>

                    <p>С уважением,</p>
                    <p>Команда RailStat</p>";

                var userEmail = new EmailContent
                {
                    To = notification.Email,
                    Subject = "Регистрация в системе «РейлСтат»",
                    Body = userBody,
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(userEmail);
                _logger.LogInformation("Email with registration instructions sent to legal user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending legal registration email");
            }
        }

        public async Task Handle(LegalUserSubscriptionUpdatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                var operation = notification.Category == SubscriptionPriceCategory.AiRequests
                    ? "Дополнительные запросы к сервису AI"
                    : "Продление основной подписки";

                var details = notification.Category == SubscriptionPriceCategory.AiRequests
                   ? $"Количество запросов: {notification.SubscriptionRequestCount}"
                   : $"Описание подписки: {notification.SubscriptionDescription}. " +
                     $"Срок: {(notification.SubscriptionDurationInMonths.HasValue ? $"{notification.SubscriptionDurationInMonths} мес." : "неизвестный")}";

                var body = $@"<h2>Поступила заявка <strong>{notification.SubmissionNumber}</strong> на обновление подписки от юридического лица</h2>
                                <p><strong>Организация:</strong> {notification.OrganizationName}</p>
                                <p><strong>ИНН:</strong> {notification.INN}</p>
                                <p><strong>КПП:</strong> {notification.KPP}</p>
                                <p><strong>ОГРН:</strong> {notification.OGRN}</p>
                                <p><strong>Email:</strong> {notification.Email}</p>
                                <p><strong>Телефон:</strong> {notification.Phone}</p>
                                <p><strong>Адрес:</strong> {notification.Address}</p>
                                <p><strong>Операция:</strong> {operation}</p>
                                <p><strong>Детали:</strong> {details}</p>
                                <p><strong>Время запроса:</strong> {notification.RequestedAt:dd.MM.yyyy HH:mm:ss}</p>";

                var emailContent = new EmailContent
                {
                    To = _emailSettings.AdminEmail,
                    Subject = $"Запрос на {operation} от юридического лица.  Номер заявки: {notification.SubmissionNumber}",
                    Body = body,
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(emailContent);
                _logger.LogInformation("Email with legal registration info sent to admin");

                var userBody = @"
                    <p>Благодарим вас за обновление подписки в системе «РейлСтат».</p>

                    <p>В настоящий момент для вашей организации формируется счёт на оплату доступа к системе.</p>
                    <p>Для корректного оформления счёта просим вас в ответном письме направить карточку предприятия (реквизиты вашей организации).</p>

                    <p>После получения реквизитов мы подготовим и направим вам счёт на указанную электронную почту.</p>

                    <p>Если у вас возникнут вопросы, пожалуйста, свяжитесь с нашей службой поддержки.</p>

                    <p>С уважением,</p>
                    <p>Команда RailStat</p>";

                var userEmail = new EmailContent
                {
                    To = notification.Email,
                    Subject = "Регистрация в системе «РейлСтат»",
                    Body = userBody,
                    IsHtml = true
                };

                await _emailService.SendEmailAsync(userEmail);
                _logger.LogInformation("Email with registration instructions sent to legal user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending legal subscription update email");
            }
        }
    }
}
