using Newtonsoft.Json;

namespace StockApp.Api.Models
{
    public enum Country
    {
        JPN = 0,
    }

    public class Company
    {
        [JsonProperty("id")]
        public string Code { get; init; }

        [JsonProperty("country")]
        public Country Country { get; init; }

        [JsonProperty("name")]
        public string Name { get; init; }
    }
}
