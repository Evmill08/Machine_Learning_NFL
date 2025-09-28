using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class EventsResponseDto
    {
        public int Count;

        [JsonPropertyName("items")]
        public List<RefDto> EventRefs;
    }
}