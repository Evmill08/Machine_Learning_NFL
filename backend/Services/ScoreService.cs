using backend.Models;
using backend.DTOs;
using backend.Utilities;

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
            var scoreResponse = await _httpClient.GetFromJsonResilientAsync<ScoreDto>(scoreRef.Ref)
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