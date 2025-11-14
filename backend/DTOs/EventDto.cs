using System.Text.Json.Serialization;
using DocumentFormat.OpenXml;
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

        [JsonPropertyName("venue")]
        public VenueDto Venue {get; set;}

        [JsonPropertyName("competitors")]
        public List<CompetitorDto> Competitors { get; set; }

        [JsonPropertyName("odds")]
        public RefDto OddsRef { get; set; }

        [JsonPropertyName("predictor")]
        public RefDto PredictorRef { get; set; }
    }

    public class VenueDto
    {
        [JsonPropertyName("fullName")]
        public string StadiumName {get; set;}

        [JsonPropertyName("address")]
        public AddressDto AddressDto {get; set;}

        [JsonPropertyName("grass")]
        public bool Grass {get; set;}

        [JsonPropertyName("indoor")]
        public bool Indoors{get; set;}
    }

    public class Coordinates
    {
        [JsonPropertyName("name")]
        public string City {get; set;}
        
        [JsonPropertyName("latitude")]
        public double Latitude {get; set;}
        
        [JsonPropertyName("longitude")]
        public double Longitude {get; set;}
    }

    public class AddressDto
    {
        [JsonPropertyName("city")]
        public string City {get; set;}

        [JsonPropertyName("state")]
        public string State {get; set;}

        [JsonPropertyName("zipCode")]
        public string zipCode {get; set;}

        [JsonPropertyName("country")]
        public string Country {get; set;}
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

