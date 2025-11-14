using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;

namespace backend.DTOs
{
    public class OddsResponseDto
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<OddsDto> Odds { get; set; }
    }

    public class OddsDto
    {
        [JsonPropertyName("provider")]
        public Provider Provider { get; set; }

        [JsonPropertyName("details")]
        public string Details { get; set; }

        [JsonPropertyName("overUnder")]
        public decimal OverUnder { get; set; }

        [JsonPropertyName("spread")]
        public decimal Spread { get; set; }
    }

    public class Provider
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string SportsBook { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }
    }
}