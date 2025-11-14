
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
    }

    public class TeamDto
    {
        public string Id;
        public string Location;
        public string Name;
        public string DisplayName;
    }
}