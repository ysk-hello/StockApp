using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace StockApp.Api.Models
{
    public class StockData
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public DateTime Date { get; init; }

        [JsonProperty("country_company_code")]
        [JsonPropertyName("country_company_code")]
        public string CountryCompanyCode { get; init; }

        [JsonProperty("open")]
        [JsonPropertyName("open")]
        public double Open { get; init; }

        [JsonProperty("high")]
        [JsonPropertyName("high")]
        public double High { get; init; }

        [JsonProperty("low")]
        [JsonPropertyName("low")]
        public double Low { get; init; }

        [JsonProperty("close")]
        [JsonPropertyName("close")]
        public double Close { get; init; }
    }
}
