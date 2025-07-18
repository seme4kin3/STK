using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using STK.Application.DTOs.AuthDto;
using STK.Application.DTOs.TBank;
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

        public async Task<TBankInitResponseDto> InitPaymentAsync(string orderId, decimal amount, string description, string notificationUrl, string email)
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
            var resp = JsonConvert.DeserializeObject<TBankInitResponseDto>(respBody);

            if (resp?.Success != true)
            {
                throw new Exception($"Ошибка при инициализации платежа: {resp.ErrorMessage ?? "Unknown error"}");
            }

            return resp;
        }
        private static string GenerateToken(Dictionary<string, object> parameters, string password)
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
