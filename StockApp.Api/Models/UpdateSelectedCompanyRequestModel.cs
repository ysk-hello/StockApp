using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StockApp.Api.Models
{
    public class UpdateSelectedCompanyRequestModel
    {
        [JsonProperty("id")]    // cosmosdb
        [JsonPropertyName("id")]    // デシリアライズ
        public string Code { get; init; }

        [JsonProperty("country")]
        [JsonPropertyName("country")]
        public Country Country { get; init; }

        [JsonProperty("is_selected")]
        [JsonPropertyName("is_selected")]
        public bool IsSelected { get; init; }

        public static UpdateSelectedCompanyRequestModel? Parse(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<UpdateSelectedCompanyRequestModel>(json);
        }
    }
}
