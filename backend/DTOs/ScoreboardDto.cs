using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class ScoreboardDto
    {
        [JsonPropertyName("week")]
        public ScoreBoardWeek scoreBoardWeek { get; set; }
    }

    public class ScoreBoardWeek
    {
        [JsonPropertyName("number")]
        public int WeekNumber { get; set; }
    }
}