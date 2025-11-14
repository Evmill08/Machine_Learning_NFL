using System.Text.Json.Serialization;

public class WeatherDto
{
    [JsonPropertyName("data")]
    public List<WeatherDataDto> WeatherData {get; set;}
}

public class WeatherDataDto
{
    [JsonPropertyName("temp")]
    public double Temperature {get; set;}

    [JsonPropertyName("feels_like")]
    public double FeelsLike {get; set;}    

    [JsonPropertyName("humidity")]
    public double Humidity {get; set;}

    [JsonPropertyName("visibility")]
    public double Visibility {get; set;}

    [JsonPropertyName("wind_speed")]
    public double WindSpeed {get; set;}

    [JsonPropertyName("wind_deg")]
    public double WindDegree {get; set;}

    [JsonPropertyName("rain")]
    public double Rain {get; set;} //mm/h

    [JsonPropertyName("snow")]
    public double Snow {get; set;} //mm/h
}