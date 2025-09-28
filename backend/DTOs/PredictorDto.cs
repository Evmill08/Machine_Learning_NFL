using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class PredictorDto
    {
        public string Name;
        public string ShortName;
        public string lastModified;

        [JsonPropertyName("homeTeam")]
        public TeamPredictionDto HomeTeamPredictions;

        [JsonPropertyName("awayTeam")]
        public TeamPredictionDto AwayTeamPredictions;
    }

    public class TeamPredictionDto
    {
        [JsonPropertyName("statistics")]
        public List<StatDto> Predictors;
    }

    public class StatDto
    {
        public string Name;
        public string DisplayName;
        public string Description;
        public decimal Value;
        public string DisplayValue;
    }
}