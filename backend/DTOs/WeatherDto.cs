using System.Text.Json.Serialization;

public class WeatherDto
{
    [JsonPropertyName("daily")]
    public WeatherDataDto WeatherData {get; set;}
}

public class WeatherDataDto
{
    [JsonPropertyName("temperature_2m_mean")]
	public List<double> MeanTemperature {get; set;}

    [JsonPropertyName("temperature_2m_max")]
	public List<double> MaxTemperature {get; set;}

    [JsonPropertyName("temperature_2m_min")]
	public List<double> MinTemperature {get; set;}

    [JsonPropertyName("apparent_temperature_mean")]
	public List<double> ApparentTemperature {get; set;}

    [JsonPropertyName("precipitation_sum")]
	public List<double> PrecipitationSum {get; set;}

    [JsonPropertyName("snowfall_sum")]
	public List<double> SnowfallSum {get; set;}

    [JsonPropertyName("precipitation_hours")]
	public List<double> PrecipitationHours {get; set;}

    [JsonPropertyName("rain_sum")]
	public List<double> RainSum {get; set;}

    [JsonPropertyName("wind_speed_10m_max")]
	public List<double> WindSpeedMax {get; set;}

    [JsonPropertyName("wind_gusts_10m_max")]
	public List<double> WindGustsMax {get; set;}

    [JsonPropertyName("wind_direction_10m_dominant")]
	public List<double> DominantWindDirection {get; set;}

    [JsonPropertyName("relative_humidity_2m_mean")]
	public List<double> MeanRelativeHumidity {get; set;}

    [JsonPropertyName("wind_gusts_10m_mean")]
	public List<double> MeanWindGusts {get; set;}

    [JsonPropertyName("wind_speed_10m_mean")]
	public List<double> MeanWindSpeed {get; set;}

}