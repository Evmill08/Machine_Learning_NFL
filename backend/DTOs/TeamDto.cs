
using System.Text.Json.Serialization;

// TODO: Add some inits to some of these, getting weird errors for missing data. 
namespace backend.DTOs
{
    public class TeamResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("displayname")]
        public string DisplayName { get; set; }

        [JsonPropertyName("record")]
        public RefDto RecordRef { get; set; }

        [JsonPropertyName("oddsRecords")]
        public RefDto OddsRecordRef { get; set; }

        [JsonPropertyName("statistics")]
        public RefDto StatisticsRef { get; set; }

        [JsonPropertyName("injuries")]
        public RefDto InjuriesRef { get; set; }
    }

    public class TeamDto
    {
        public string Id;
        public string Location;
        public string Name;
        public string DisplayName;
        public RecordResponseDto Record;
        public OddsRecordDto OddsRecord;
        public StatisticsDto Statistics;
        public InjuriesDto Injuries;
    }

    public class RecordResponseDto
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<RecordDto> Records { get; set; }
    }

    public class OddsRecordDto
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("items")]
        public List<BookOddsRecord> BookOddsRecords { get; set; }
    }

    public class StatisticsDto
    {
        [JsonPropertyName("splits")]
        public SplitsDto Splits { get; set; }
    }

    public class InjuriesDto
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        // We Can make this more impactful by going through each injury
        // and maybe coming to some "injury heuristic" to use based on 
        // who is injured?
    }

    public class SplitsDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("categories")]
        public List<StatCategoryDto> StatCategories { get; set; }
    }

    public class StatCategoryDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("stats")]
        public List<CategoryStatDto> CategoryStats { get; set; }
    }

    public class CategoryStatDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("abbreviation")]
        public string Abbreviation { get; set; }

        [JsonPropertyName("rank")]
        public int Rank { get; set; }
    }

    public class BookOddsRecord
    {
        [JsonPropertyName("abbreviation")]
        public string Abbreviation { get; set; }

        [JsonPropertyName("shortDisplayName")]
        public string ShortDisplayName { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("stats")]
        public List<OddsStatDto> OddsStats { get; set; }
    }

    public class OddsStatDto
    {
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("abbreviation")]
        public string Abbreviation { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("dispayValue")]
        public string DisplayValue { get; set; }
    }

    public class RecordDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("stats")]
        public List<StatDto> Stats { get; set; }
    }
}