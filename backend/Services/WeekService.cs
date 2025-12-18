using backend.Utilities;
using backend.DTOs;
using backend.Models;

// TODO: Optimize this service
namespace backend.Services
{
    public interface IWeeksService
    {
        public Task<List<Week>> GetAllWeeksForYearAsync(int seasonYear, CancellationToken cancellationToken = default);
        public Task<List<Week>> GetCompletedWeeksForCurrentYearAsync(int currentYear, CancellationToken cancellationToken = default);
        public Task<WeekDto> GetWeekByWeekNumberAsync(int seasonYear, int weekNumber);
        public Task<Week> GetWeekDataByWeekNumberAsync(int seasonYear, int weekNumber);
        public Task<Week> GetWeekByRefAsync(RefDto weekRef);
        public Task<int> GetWeekNumberAsync();
    }

    public class WeeksService : IWeeksService
    {
        private readonly HttpClient _httpClient;
        private readonly Lazy<IEventService> _eventServiceLazy;

        public WeeksService(HttpClient httpClient, Lazy<IEventService> eventServiceLazy)
        {
            _httpClient = httpClient;
            _eventServiceLazy = eventServiceLazy;
        }

        public async Task<List<Week>> GetAllWeeksForYearAsync(int seasonYear, CancellationToken cancellationToken = default)
        {
            var topLevelUrl = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks?lang=en&region=us";

            var weekResponse = await _httpClient.GetFromJsonResilientAsync<WeeksResponseDto>(topLevelUrl)
                ?? throw new Exception($"Error fetching weeks for {seasonYear}");

            var maxConcurrency = Environment.ProcessorCount;
            var throttler = new SemaphoreSlim(maxConcurrency);

            var weekTasks = weekResponse.WeekRefs.Select(async weekRef =>
            {
                await throttler.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    return await GetWeekByRefAsync(weekRef).ConfigureAwait(false);
                }
                finally
                {
                    throttler.Release();
                }
            });

            var weeks = await Task.WhenAll(weekTasks).ConfigureAwait(false);

            return weeks
                .Where(w => w != null)
                .OrderBy(w => w.WeekNumber)
                .ToList();
        }

        public async Task<List<Week>> GetCompletedWeeksForCurrentYearAsync(int currentYear, CancellationToken cancellationToken = default)
        {
            var currentWeek = await GetWeekNumberAsync();
            var weekList = new List<Week>();
            for (int i = 0; i < currentWeek; ++i)
            {
                var weekData = await GetWeekDataByWeekNumberAsync(currentYear, i);
                weekList.Add(weekData);
            }
            return weekList;
        }

        public async Task<WeekDto> GetWeekByWeekNumberAsync(int seasonYear, int weekNumber)
        {
            var url = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks/{weekNumber}?lang=en&region=us";

            var response = await _httpClient.GetFromJsonResilientAsync<WeekDto>(url)
                ?? throw new Exception($"Week data not found for week {weekNumber} of the {seasonYear} season");

            return response;
        }

        public async Task<Week> GetWeekDataByWeekNumberAsync(int seasonYear, int weekNumber)
        {
            var response = await GetWeekByWeekNumberAsync(seasonYear, weekNumber);

            var events = await _eventServiceLazy.Value.GetEventsByRefAsync(response.EventRefs);

            return new Week
            {
                WeekNumber = response.Number,
                StartDate = DateTime.Parse(response.StartDate, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                EndDate = DateTime.Parse(response.EndDate, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                Events = [.. events]
            };
        }

        public async Task<Week> GetWeekByRefAsync(RefDto weekRef)
        {
            var weekResponse = await _httpClient.GetFromJsonResilientAsync<WeekDto>(weekRef.Ref)
                ?? throw new Exception("Error fecthing week by week reference");

            // Resolve IEventService lazily only when needed
            var events = await _eventServiceLazy.Value.GetEventsByRefAsync(weekResponse.EventRefs);

            return new Week
            {
                WeekNumber = weekResponse.Number,
                StartDate = DateTime.Parse(weekResponse.StartDate, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                EndDate = DateTime.Parse(weekResponse.EndDate, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                Events = [.. events]
            };
        }

        public async Task<int> GetWeekNumberAsync()
        {
            var url = "https://site.api.espn.com/apis/site/v2/sports/football/nfl/scoreboard";

            var response = await _httpClient.GetFromJsonResilientAsync<ScoreboardDto>(url);
            return response?.scoreBoardWeek.WeekNumber
                ?? throw new Exception("Error fetching week number");
        }
    }
}