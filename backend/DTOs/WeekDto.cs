using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class WeekDto
    {
        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("startDate")]
        public string StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public string EndDate { get; set; }

        [JsonPropertyName("events")]
        public RefDto EventRefs { get; set; }
    }
}