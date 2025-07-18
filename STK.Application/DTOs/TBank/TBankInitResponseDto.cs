using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace STK.Application.DTOs.TBank
{
    public class TBankInitResponseDto : TBankBaseResponseDto
    {
        /// <summary>
        /// Сумма в копейках
        /// </summary>
        [JsonProperty("Amount")]
        [JsonPropertyName("Amount")]
        public long Amount { get; set; }

        /// <summary>
        /// Номер заказа в системе продавца
        /// </summary>
        [JsonProperty("OrderId")]
        [JsonPropertyName("OrderId")]
        public string OrderId { get; set; }

        /// <summary>
        /// Статус транзакции
        /// </summary>
        [JsonProperty("Status")]
        [JsonPropertyName("Status")]
        public string Status { get; set; }

        /// <summary>
        /// Уникальный идентификатор транзакции в системе банка
        /// В ответах Init приходит как строка
        /// </summary>
        [JsonProperty("PaymentId")]
        [JsonPropertyName("PaymentId")]
        public string PaymentId { get; set; }

        /// <summary>
        /// Ссылка на страницу оплаты
        /// По умолчанию ссылка доступна в течение 24 часов
        /// </summary>
        [JsonProperty("PaymentURL")]
        [JsonPropertyName("PaymentURL")]
        public string PaymentURL { get; set; }

        /// <summary>
        /// Сумма в рублях (автоматически рассчитывается из копеек)
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public decimal AmountInRubles => Amount / 100m;

        /// <summary>
        /// Проверяет, был ли платеж успешно инициализирован
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public bool IsInitialized => Success && !string.IsNullOrEmpty(PaymentURL);
    }
}
