using System.Net.WebSockets;
using System.Runtime.InteropServices.Marshalling;
using backend.DTOs;
using backend.Models;
using backend.Utilities;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace backend.Services
{
    public interface IWeatherService
    {
        public Task<Weather> GetWeatherForCompetitionAsync(string city, string country, DateTime date);
    }

    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _nijasApiKey;
        private readonly string _openWeatherApiKey;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _nijasApiKey = Environment.GetEnvironmentVariable("API_NINJAS_KEY");
            _openWeatherApiKey = Environment.GetEnvironmentVariable("API_OPEN_WEATHER_KEY");
        }

        public async Task<Weather> GetWeatherForCompetitionAsync(string city, string country, DateTime date)
        {
            var (latitude, longitude) = await GetCoordinatesForCompetitionAsync(city, country);

            var utcTime = date.ToUniversalTime();
            var timestamp = new DateTimeOffset(utcTime).ToUnixTimeSeconds();

            var url = $"https://api.openweathermap.org/data/3.0/onecall/timemachine?lat={latitude}&lon={longitude}&dt={timestamp}&appid={_openWeatherApiKey}&units=imperial";

            var weatherResponse = await _httpClient.GetFromJsonResilientAsync<WeatherDto>(url);

            var WeatherData = weatherResponse.WeatherData.FirstOrDefault();

            return new Weather
            {
                Temperature = WeatherData.Temperature,
                FeelsLike = WeatherData.FeelsLike,
                Humidity = WeatherData.Humidity,
                Visibility = WeatherData.Visibility, // TODO: This might be in KM we need imperial units
                WindSpeed = WeatherData.WindSpeed,
                WindDegree = WeatherData.WindDegree,
                Rain = (0.03937713512277) * WeatherData.Rain, // Converts to in/h
                Snow = (0.03937713512277) * WeatherData.Snow,
            };
            
        }

        private async Task<(double, double)> GetCoordinatesForCompetitionAsync(string city, string country)
        {
            var url = $"https://api.api-ninjas.com/v1/geocoding?city={city}&country={country}";

            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _nijasApiKey);

            try
            {
                var response = await _httpClient.GetFromJsonResilientAsync<List<Coordinates>>(url)
                    ?? throw new Exception($"Error fetching coordinates for {city}, {country}");

                var coords = response
                    .FirstOrDefault(c => string.Equals(c.City, city, StringComparison.OrdinalIgnoreCase))
                    ?? response.FirstOrDefault()
                    ?? throw new Exception($"No coordinates found for {city}, {country}");

                return (coords.Latitude, coords.Longitude);
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Remove("X-Api-Key");
            }
        }
    }
}
