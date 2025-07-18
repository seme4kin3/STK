using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;


namespace STK.Application.Services
{
    public class TBankPaymentService
    {
        private readonly HttpClient _client;
        private readonly string _terminalKey;
        private readonly string _password;
        private readonly IEmailService _emailService;

        public TBankPaymentService(HttpClient client, IConfiguration config, IEmailService emailService)
        {
            _client = client;
            _terminalKey = "1752526672336DEMO";
            _password = "JLrExNCw&n!e2p01";
            //_terminalKey = config["TBank:TerminalKey"];
            //_password = config["TBank:Password"];
            _emailService = emailService;
        }

        //public async Task<string> InitPaymentAsync(string orderId, decimal amount, string description, string notificationUrl, string email)
        //{
        //    var url = "https://securepay.tinkoff.ru/v2/Init";
        //    var amountKop = (int)(amount * 100);

        //    // Токен согласно документации: TerminalKey+Amount+OrderId+Password (MD5)
        //    //var tokenStr = $"{_terminalKey}{amountKop}{orderId}{_password}";
        //    //var md5 = System.Security.Cryptography.MD5.Create();
        //    //var tokenBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(tokenStr));
        //    //var token = BitConverter.ToString(tokenBytes).Replace("-", "").ToLower();

        //    var token = GenerateTinkoffToken(_terminalKey, amountKop.ToString(), orderId, _password);

        //    var body = new
        //    {
        //        TerminalKey = _terminalKey,
        //        Amount = amountKop,
        //        OrderId = orderId,
        //        Description = description,
        //        NotificationURL = notificationUrl,
        //        DATA = new { Email = email },
        //        Token = token
        //    };

        //    var json = JsonConvert.SerializeObject(body);

        //    var response = await _client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        throw new HttpRequestException($"HTTP error: {response.StatusCode}");
        //    }
        //    var respBody = await response.Content.ReadAsStringAsync();
        //    dynamic resp = JsonConvert.DeserializeObject(respBody);

        //    if (resp.Success != true)
        //        throw new Exception("Ошибка при инициализации платежа: " + resp.Message);

        //    return resp.PaymentURL;
        //}

        //public async Task SendPaymentUrlToEmail(string email, string url)
        //{
        //    await _emailService.SendAsync(email, "Оплата за сервис", $"Ваша ссылка для оплаты: {url}");
        //}

        public async Task<string> InitPaymentAsync(string orderId, decimal amount, string description, string notificationUrl, string email)
        {
            //var url = "https://securepay.tinkoff.ru/v2/Init";

            var url = "https://rest-api-test.tinkoff.ru/v2/Init";

            var amountKop = (int)(amount * 100);

            // Формируем параметры запроса
            var requestParams = new Dictionary<string, object>
            {
                ["TerminalKey"] = _terminalKey,
                ["Amount"] = amountKop,
                ["OrderId"] = orderId,
                ["Description"] = description,
                ["NotificationURL"] = notificationUrl,
                //["DATA"] = new { Email = email }
            };

            // Генерируем токен согласно новой спецификации
            var token = GenerateToken(requestParams, _password);

            // Добавляем токен в параметры запроса
            requestParams["Token"] = token;

            var json = JsonConvert.SerializeObject(requestParams);

            var response = await _client.PostAsync(url,
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP error: {response.StatusCode}");
            }

            var respBody = await response.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<dynamic>(respBody);

            if (resp?.Success != true)
            {
                throw new Exception($"Ошибка при инициализации платежа: {resp?.Message ?? "Unknown error"}");
            }

            return resp.PaymentURL;
        }
        public static string GenerateToken(Dictionary<string, object> parameters, string password)
        {
            // 1. Добавляем пароль к параметрам
            var tokenData = new Dictionary<string, object>(parameters)
            {
                ["Password"] = password
            };

            // 2. Сортируем параметры по имени в алфавитном порядке
            var sortedParams = tokenData
                .OrderBy(p => p.Key, StringComparer.Ordinal)
                .ToDictionary(p => p.Key, p => p.Value);

            // 3. Объединяем все значения в одну строку
            var valuesString = string.Concat(sortedParams.Select(p =>
                p.Value is string ? p.Value.ToString() :
                JsonConvert.SerializeObject(p.Value)));

            // 4. Вычисляем SHA-256 хэш
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(valuesString));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }

}
