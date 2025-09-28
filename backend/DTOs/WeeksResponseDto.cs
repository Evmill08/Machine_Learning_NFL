using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class WeeksResponseDto
    {
        public int Count;

        [JsonPropertyName("items")]
        public List<RefDto> WeekRefs;
    }
}