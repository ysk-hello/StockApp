using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace StockApp.Api.Models
{
    public class Data
    {
        [JsonProperty("date")]
        [JsonPropertyName("date")]
        public DateTime Date { get; init; }

        [JsonProperty("value")]
        [JsonPropertyName("value")]
        public double Value { get; init; }
    }
}
