using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class WeekDto
    {
        public int Number;
        public string StartDate;
        public string EndDate;

        [JsonPropertyName("events")]
        public RefDto EventsRefs;

        [JsonPropertyName("qbr")]
        public RefDto QbrRef; // May not need, or shouldn't be here maybe
    }
}