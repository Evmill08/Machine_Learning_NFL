using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class EventsResponseDto
    {
        [JsonPropertyName("$meta")]
        public EventMetadataDto EventMetadata { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<RefDto> EventRefs { get; set; }
    }

    public class EventMetadataDto
    {
        [JsonPropertyName("parameters")]
        public EventParametersDto EventParametersDto { get; set; }
    }

    public class EventParametersDto
    {
        [JsonPropertyName("week")]
        public List<string> WeekString { get; set; }

        [JsonPropertyName("season")]
        public List<string> SeasonString { get; set; }
    }
}