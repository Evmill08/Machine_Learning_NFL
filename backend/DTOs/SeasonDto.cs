using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class SeasonDto
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("startDate")]
        public string StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public string EndDate { get; set; }

        [JsonPropertyName("type")]
        public SeasonTypeDto Type { get; set; }
    }

    public class SeasonTypeDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("week")]
        public RefDto WeekRef { get; set; }

        [JsonPropertyName("weeks")]
        public RefDto WeeksRef { get; set; }
    }

    public class RefDto
    {
        [JsonPropertyName("$ref")]
        public string Ref { get; set; }
    }
}
