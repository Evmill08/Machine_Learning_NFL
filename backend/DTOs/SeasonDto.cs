using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class SeasonDto
    {
        public int Year;
        public string StartDate;
        public string EndDate;
        public SeasonTypeDto SeasonType;
    }

    public class SeasonTypeDto
    {
        public string Id;
        public int Type;
        public RefDto GroupRef;
        public RefDto WeeksRef;
    }

    public class RefDto
    {
        [JsonPropertyName("$ref")]
        public string Ref;
    }
}