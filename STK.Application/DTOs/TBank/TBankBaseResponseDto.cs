using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace STK.Application.DTOs.TBank
{
    public class TBankBaseResponseDto
    {
        /// <summary>
        /// Идентификатор терминала, выдается продавцу банком
        /// </summary>
        [JsonProperty("TerminalKey")]
        [JsonPropertyName("TerminalKey")]
        public string TerminalKey { get; set; }

        /// <summary>
        /// Успешность операции
        /// </summary>
        [JsonProperty("Success")]
        [JsonPropertyName("Success")]
        public bool Success { get; set; }

        /// <summary>
        /// Код ошибки, "0" - если успешно
        /// </summary>
        [JsonProperty("ErrorCode")]
        [JsonPropertyName("ErrorCode")]
        public string ErrorCode { get; set; }

        /// <summary>
        /// Краткое описание ошибки
        /// </summary>
        [JsonProperty("Message")]
        [JsonPropertyName("Message")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Подробное описание ошибки
        /// </summary>
        [JsonProperty("Details")]
        [JsonPropertyName("Details")]
        public string ErrorDetails { get; set; }
    }
}
