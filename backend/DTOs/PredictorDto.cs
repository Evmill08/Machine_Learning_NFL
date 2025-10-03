using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class PredictorDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("shortName")]
        public string ShortName { get; set; }

        [JsonPropertyName("lastModified")]
        public string LastModified { get; set; }

        [JsonPropertyName("homeTeam")]
        public TeamPredictionDto HomeTeamPredictions { get; set; }

        [JsonPropertyName("awayTeam")]
        public TeamPredictionDto AwayTeamPredictions { get; set; }
    }

    public class TeamPredictionDto
    {
        [JsonPropertyName("statistics")]
        public List<StatDto> Predictors { get; set; }
    }

    public class StatDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("displayValue")]
        public string DisplayValue { get; set; }
    }
}