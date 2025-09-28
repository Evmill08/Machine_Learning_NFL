using backend.DTOs;

// TODO: Change the seasonDto type to a Season Model once we figure out details
namespace backend.Services
{
    public interface ISeasonService
    {
        public Task<Season> GetSeasonByYearAsync(int seasonYear);
        public Task<IEnumerable<Season>> GetSeasonsRangedAsync(int startYear, int endYear);
    }

    public class SeasonService : ISeasonService
    {
        private readonly HttpClient _httpClient;

        public SeasonService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Season> GetSeasonByYearAsync(int seasonYear)
        {
            var url = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}?lang=en&region=us";

            var response = await _httpClient.GetFromJsonAsync<SeasonDto>(url)
                ?? throw new Exception($"Error fetching {seasonYear} season data.");

            // TODO: We're going to need a way to get the weeks and group from their refs here when converting from seasonDto to Season
            return response;
        }

        // TODO: We need to figure out how many years we want to get here
        public async Task<IEnumerable<SeasonDto>> GetSeasonsRangedAsync(int startYear, int endYear)
        {
            var count = endYear - startYear + 1;
            var yearSequence = Enumerable.Range(startYear, count);

            var seasonList = new List<SeasonDto>();

            foreach (var year in yearSequence)
            {
                var season = await GetSeasonByYearAsync(year);
                seasonList.Add(season);
            }

            return seasonList;
        }
    }
}