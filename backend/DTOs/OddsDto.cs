using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class OddsResponseDto
    {
        public int Count;

        [JsonPropertyName("items")]
        public List<OddsDto> Odds;
    }

    public class OddsDto
    {
        public string Details;
        public decimal OverUnder;
        public decimal Spread;
        public bool MoneyLineWinner;
        public bool SpreadWinner;
    }
}