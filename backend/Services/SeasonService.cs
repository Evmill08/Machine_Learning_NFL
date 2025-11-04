using backend.DTOs;
using backend.Models;
using backend.Utilities;

// TODO: Optimize this service
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
        private readonly IWeeksService _weeksService;

        public SeasonService(HttpClient httpClient, IWeeksService weeksService)
        {
            _httpClient = httpClient;
            _weeksService = weeksService;
        }

        public async Task<Season> GetSeasonByYearAsync(int seasonYear)
        {
            var url = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/?lang=en&region=us";

            var response = await _httpClient.GetFromJsonResilientAsync<SeasonDto>(url)
                ?? throw new Exception($"Error fetching {seasonYear} season data.");

            var weeks = await _weeksService.GetAllWeeksForYearAsync(response.Year);

            return new Season
            {
                Year = response.Year,
                StartDate = DateTime.Parse(response.StartDate, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                EndDate = DateTime.Parse(response.EndDate, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                SeasonType = new SeasonType
                {
                    Id = response.Type.Id,
                    Type = response.Type.Type,
                    Weeks = weeks.ToList(),
                }
            };
        }

        // TODO: We need to figure out how many years we want to get here
        public async Task<IEnumerable<Season>> GetSeasonsRangedAsync(int startYear, int endYear)
        {
            var count = endYear - startYear + 1;
            var yearSequence = Enumerable.Range(startYear, count);

            var seasons = new List<Season>();

            using var semaphore = new SemaphoreSlim(4);

            var tasks = yearSequence.Select(async year =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var season = await GetSeasonByYearAsync(year);
                    lock (seasons) seasons.Add(season);
                }
                finally { semaphore.Release(); }
            });

            await Task.WhenAll(tasks);

            return seasons.OrderBy(s => s.Year);
        }
    }
}