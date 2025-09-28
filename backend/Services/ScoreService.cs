using backend.Models;
using backend.DTOs;

namespace backend.Services
{
    public interface IScoreService
    {
        public Task<Score> GetTeamScoreAsync(RefDto scoreRef);
    }

    public class ScoreService : IScoreService
    {
        private readonly HttpClient _httpClient;

        public ScoreService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Score> GetTeamScoreAsync(RefDto scoreRef)
        {
            var scoreResponse = await _httpClient.GetFromJsonAsync<ScoreDto>(scoreRef.Ref)
                ?? throw new Exception("Error fetching score for competitor");

            return new Score
            {
                Value = scoreResponse.Value,
                DisplayValue = scoreResponse.DisplayValue,
                Winner = scoreResponse.Winner
            };
        }
    }
}