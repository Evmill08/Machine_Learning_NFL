using backend.DTOs;
using backend.Models;
using backend.Utilities;
using System.Net.Http.Headers;

using Microsoft.Extensions.Caching.Memory;

namespace backend.Services
{
    public interface IWeatherService
    {
        public Task<Weather> GetWeatherForCompetitionAsync(string city, string country, DateTime date);
    }

    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string _nijasApiKey;
        private readonly string _openWeatherApiKey;

        public WeatherService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
            _nijasApiKey = Environment.GetEnvironmentVariable("API_NINJAS_KEY");
        }

        public async Task<Weather> GetWeatherForCompetitionAsync(string city, string country, DateTime date)
        {
            var (latitude, longitude) = await GetCoordinatesForCompetitionAsync(city, country);

            var utcDate = date.ToUniversalTime().Date;
            var utcToday = DateTime.UtcNow.Date;

            bool forecast = utcDate >= utcToday;

            var weatherData = await GetAppropriateWeatherDataAsync(latitude, longitude, date, forecast);
            return weatherData;
        }

        private async Task<Weather> GetAppropriateWeatherDataAsync(double lat, double lon, DateTime date, bool forecast)
        {
            string start = date.ToString("yyyy-MM-dd");

            string end = start;
            string url = "";

            if (forecast)
            {
                url =
                    $"https://api.open-meteo.com/v1/forecast" +
                    $"?latitude={lat}" +
                    $"&longitude={lon}" +
                    $"&start_date={start}" +
                    $"&end_date={end}" +
                    $"&daily=temperature_2m_mean,temperature_2m_max,temperature_2m_min," +
                    $"apparent_temperature_mean,precipitation_sum,snowfall_sum,precipitation_hours," +
                    $"rain_sum,wind_speed_10m_max,wind_gusts_10m_max,wind_direction_10m_dominant," +
                    $"relative_humidity_2m_mean,wind_gusts_10m_mean,wind_speed_10m_mean";
            } else
            {
                url =
                    $"https://archive-api.open-meteo.com/v1/archive" +
                    $"?latitude={lat}" +
                    $"&longitude={lon}" +
                    $"&start_date={start}" +
                    $"&end_date={end}" +
                    $"&daily=temperature_2m_mean,temperature_2m_max,temperature_2m_min," +
                    $"apparent_temperature_mean,precipitation_sum,snowfall_sum,precipitation_hours," +
                    $"rain_sum,wind_speed_10m_max,wind_gusts_10m_max,wind_direction_10m_dominant," +
                    $"relative_humidity_2m_mean,wind_gusts_10m_mean,wind_speed_10m_mean";
            }

            var dto = await _httpClient.GetFromJsonResilientAsync<WeatherDto>(url)
                    ?? throw new Exception("Failed to deserialize Open-Meteo archive response.");

            if (dto.WeatherData == null)
            {
                throw new Exception("Open-Meteo returned no historical daily data.");
            }

            var weatherData = dto.WeatherData;

            return new Weather
            {
                MeanTemperature = weatherData.MeanTemperature[0],
                MaxTemperature = weatherData.MaxTemperature[0] ,
                MinTemperature = weatherData.MinTemperature[0] ,
                ApparentTemperature = weatherData.ApparentTemperature[0] ,
                PrecipitationSum = weatherData.PrecipitationSum[0] ,
                SnowfallSum = weatherData.SnowfallSum[0] ,
                PrecipitationHours = weatherData.PrecipitationHours[0] ,
                RainSum = weatherData.RainSum[0] ,
                WindSpeedMax = weatherData.WindSpeedMax[0] ,
                WindGustsMax = weatherData.WindGustsMax[0] ,
                DominantWindDirection = weatherData.DominantWindDirection[0] ,
                MeanRelativeHumidity = weatherData.MeanRelativeHumidity[0] ,
                MeanWindGusts = weatherData.MeanWindGusts[0] ,
                MeanWindSpeed = weatherData.MeanWindSpeed[0] ,
            };
        }

        private async Task<(double, double)> GetCoordinatesForCompetitionAsync(string city, string country)
        {
            var cacheKey = $"coords:{city}:{country}";

            if (_cache.TryGetValue(cacheKey, out (double Latitude, double Longitude) coordinates))
            {
                return coordinates;
            }

            // TODO: Figure this out: API key is in the headers but isn't being registered
            var url = $"https://api.api-ninjas.com/v1/geocoding?city={city}&country={country}&{_nijasApiKey}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-api-key", _nijasApiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var coordsList = await response.Content.ReadFromJsonAsync<List<Coordinates>>()
                ?? throw new Exception($"Error fetching coordinates for {city}, {country}");

            var coords = coordsList
                .FirstOrDefault(c => string.Equals(c.City, city, StringComparison.OrdinalIgnoreCase))
                ?? coordsList.FirstOrDefault()
                ?? throw new Exception($"No coordinates found for {city}, {country}");

            var coordinateTuple = (coords.Latitude, coords.Longitude);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(30)); 

            _cache.Set(cacheKey, coordinateTuple, cacheOptions);

            return coordinateTuple;
        }
    }
}
