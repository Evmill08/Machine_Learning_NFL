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
        [JsonPropertyName("details")]
        public string Details { get; set; }

        [JsonPropertyName("overUnder")]
        public decimal OverUnder { get; set; }

        [JsonPropertyName("spread")]
        public decimal Spread { get; set; }

        [JsonPropertyName("moneyLineWinner")]
        public bool MoneyLineWinner { get; set; }

        [JsonPropertyName("spreadWinner")]
        public bool SpreadWinner { get; set; }
    }
}