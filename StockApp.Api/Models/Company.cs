using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace StockApp.Api.Models
{
    public enum Country
    {
        JPN = 0,
    }

    public class Company
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Code { get; init; }

        [JsonProperty("country")]
        [JsonPropertyName("country")]
        public Country Country { get; init; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonProperty("is_selected")]
        [JsonPropertyName("is_selected")]
        public bool IsSelected { get; set; }
    }
}
