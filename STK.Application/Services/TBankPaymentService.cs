using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
        private readonly string _url;
        private readonly string _notificationUrl;

        public TBankPaymentService(HttpClient client, IConfiguration config, IEmailService emailService)
        {
            _client = client;
            _terminalKey = config["TBankSettings:TerminalKey"];
            _password = config["TBankSettings:Password"];
            _url = config["TBankSettings:UrlRequest"];
            _notificationUrl = config["TBankSettings:NotificationURL"];
        }

        public async Task<TBankInitResponseDto> InitPaymentAsync(string orderId, decimal amount, string description, string email)
        {
            //var url = "https://securepay.tinkoff.ru/v2/Init";

            //var url = "https://rest-api-test.tinkoff.ru/v2/Init";

            var amountKop = (int)(amount * 100);

            // Формируем параметры запроса
            var requestParams = new Dictionary<string, object>
            {
                ["TerminalKey"] = _terminalKey,
                ["Amount"] = amountKop,
                ["OrderId"] = orderId,
                ["Description"] = description,
                ["NotificationURL"] = _notificationUrl
            };

            // Генерируем токен согласно новой спецификации
            var token = GenerateToken(requestParams, _password);

            // Добавляем токен в параметры запроса
            requestParams["Token"] = token;
            requestParams["DATA"] = new {Email = email};
            requestParams["Receipt"] = GenerateReceipt(email, string.Empty, amountKop);

            var json = JsonConvert.SerializeObject(requestParams);

            var response = await _client.PostAsync(_url,
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

        private Dictionary<string, object> GenerateReceipt(string email, string phone, decimal amountRub)
        {
            //int amountKop = (int)(amountRub * 100);
            string itemName;
            if (amountRub == 6000000)
            {
                itemName = "Подписка к сервису РейлСтат на 3 месяца";
            }
            else if (amountRub == 12000000)
            {
                itemName = "Подписка к сервису РейлСтат на 1 год";
            }
            else if (amountRub == 490000)
            {
                itemName = "Пакет 30 запросов к AI-чату RailStat (на месяц)";
            }
            else if (amountRub == 1390000)
            {
                itemName = "Пакет 100 запросов к AI-чату RailStat (на месяц)";
            }
            else if (amountRub == 3490000)
            {
                itemName = "Пакет 300 запросов к AI-чату RailStat (на месяц)";
            }
            else
            {
                // Если сумма не совпадает с известными тарифами, используем общее название
                throw new Exception($"Ошибка при формировании чека");
            }
            var receiptItem = new
            {
                Name = itemName,
                Price = amountRub,
                Quantity = 1.0,
                Amount = amountRub,
                Tax = "none",
                PaymentMethod = "full_payment",
                PaymentObject = "service"
            };

            var companyInfo = new
            {
                Email = "admin@rail-stat.ru",
                Name = "ОБЩЕСТВО С ОГРАНИЧЕННОЙ ОТВЕТСТВЕННОСТЬЮ \"ПАРТНЕРВИЗОР\"",
                Inn = "9721253967",
                PaymentAddress = "109428, г. Москва, пр-кт Рязанский, д. 10, стр. 2, помещ. 5/5/3"
            };

            return new Dictionary<string, object>
            {
                ["Email"] = email ?? "",
                ["Phone"] = phone ?? "",
                ["Taxation"] = "usn_income",
                ["Items"] = new[] { receiptItem },
                ["Company"] = companyInfo
            };
        }
    }

}
