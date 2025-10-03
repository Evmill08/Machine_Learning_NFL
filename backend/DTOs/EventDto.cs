using System.Text.Json.Serialization;
// TODO: For ALL DTOs, I need to align the names of the properties
// to what the API returns, and use JsonPropertyName for every property
// Also need to change all field to properties by adding getters and initers



namespace backend.DTOs
{
    public class EventDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("timeValid")]
        public bool TimeValid { get; set; }

        [JsonPropertyName("competitions")]
        public List<CompetitionDto> Competitions { get; set; }
    }

    public class CompetitionDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("timeValid")]
        public bool TimeValid { get; set; }

        [JsonPropertyName("dateValid")]
        public bool DateValid { get; set; }

        [JsonPropertyName("nuetralSite")]
        public bool NuetralSite { get; set; }

        [JsonPropertyName("divisionCompetition")]
        public bool DivisionCompetition { get; set; }

        [JsonPropertyName("conferenceCompetition")]
        public bool ConferenceCompetition { get; set; }

        [JsonPropertyName("competitors")]
        public List<CompetitorDto> Competitors { get; set; }

        [JsonPropertyName("odds")]
        public RefDto OddsRef { get; set; }

        [JsonPropertyName("predictor")]
        public RefDto PredictorRef { get; set; }
    }

    public class CompetitorDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("homeAway")]
        public string HomeAway { get; set; }

        [JsonPropertyName("winner")]
        public bool Winner { get; set; }

        [JsonPropertyName("team")]
        public RefDto TeamRef { get; set;}

        [JsonPropertyName("score")]
        public RefDto ScoreRef { get; set; }
    }

    public class ScoreDto
    {
        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("displayValue")]
        public string DisplayValue { get; set; }

        [JsonPropertyName("winner")]
        public bool Winner { get; set; }
    }
}

