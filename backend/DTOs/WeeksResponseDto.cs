using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class WeeksResponseDto
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<RefDto> WeekRefs { get; set; }
    }
}