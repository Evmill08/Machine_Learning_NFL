using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class EventDto
    {
        public string Id;
        public string Date;
        public string Name;
        public bool TimeValid;

        [JsonPropertyName("competitions")]
        public List<CompetitionDto> Competitions;

        public string Season;
        public string Week;
    }

    public class CompetitionDto
    {
        public string Id;
        public string Date;
        public bool TimeValid;
        public bool DateValid;
        public bool NuetralSite;
        public bool DivisionCompetition;
        public bool ConferenceCompetition;

        [JsonPropertyName("competitors")]
        public List<CompetitorDto> Competitors;

        [JsonPropertyName("odds")]
        public RefDto OddsRef;

        [JsonPropertyName("predictor")]
        public RefDto PredictorRef;
    }

    public class CompetitorDto
    {
        public string Id;
        public string HomeAway;
        public bool Winner;

        [JsonPropertyName("team")]
        public RefDto TeamRef;

        [JsonPropertyName("score")]
        public RefDto ScoreRef;
    }

    public class ScoreDto
    {
        public decimal Value;
        public string DisplayValue;
        public bool Winner;
    }
}

