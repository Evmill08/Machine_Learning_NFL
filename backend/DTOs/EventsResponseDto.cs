using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class EventsResponseDto
    {
        [JsonPropertyName("$meta")]
        public EventMetadataDto EventMetadata;
        public int Count;

        [JsonPropertyName("items")]
        public List<RefDto> EventRefs;
    }

    public class EventMetadataDto
    {
        [JsonPropertyName("parameters")]
        public EventParametersDto EventParametersDto;
    }

    public class EventParametersDto
    {
        [JsonPropertyName("week")]
        public List<string> WeekString;

        [JsonPropertyName("season")]
        public List<string> SeasonString;
    }
}