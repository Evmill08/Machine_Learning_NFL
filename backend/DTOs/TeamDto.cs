
using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class TeamResponseDto
    {
        public string Id;
        public string Location;
        public string Name;
        public string DisplayName;

        [JsonPropertyName("record")]
        public RefDto RecordRef;

        [JsonPropertyName("oddsRecords")]
        public RefDto OddsRecordRef;

        [JsonPropertyName("statistics")]
        public RefDto StatsRef;

        [JsonPropertyName("injuries")]
        public RefDto InjuriesRef;
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
        public int Count;

        [JsonPropertyName("items")]
        public List<RecordDto> Records;
    }

    public class OddsRecordDto
    {
        public int Count;

        [JsonPropertyName("items")]
        public List<BookOddsRecord> OddsRecords;
    }

    public class StatisticsDto
    {
        public SplitsDto Splits;
    }

    public class InjuriesDto
    {
        public int Count;

        // We Can make this more impactful by going through each injury
        // and maybe coming to some "injury heuristic" to use based on 
        // who is injured?
    }

    public class SplitsDto
    {
        public string Id;
        public string Name;
        public List<StatCategoryDto> StatCategories;
    }

    public class StatCategoryDto
    {
        public string Name;
        public string DisplayName;

        [JsonPropertyName("stats")]
        public List<CategoryStatDto> CategoryStats;
    }

    public class CategoryStatDto
    {
        public string Name;
        public string DisplayName;
        public decimal Value;
        public string Abbreviation;
        public int Rank;
    }

    public class BookOddsRecord
    {
        public string Abbreviation;
        public string ShortDisplayName;
        public string Type;
        public List<OddsStatDto> OddsRecordStats;
    }

    public class OddsStatDto
    {
        public string DisplayName;
        public string Abbreviation;
        public string Type;
        public decimal Value;
        public string DisplayValue;
    }

    public class RecordDto
    {
        public string Id;
        public string Name;
        public string Summary;
        public decimal Value;

        [JsonPropertyName("stats")]
        public List<StatDto> RecordStats;
    }


}