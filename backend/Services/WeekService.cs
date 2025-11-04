using backend.Utilities;
using backend.DTOs;
using backend.Models;

// TODO: Optimize this service
namespace backend.Services
{
    public interface IWeeksService
    {
        public Task<IEnumerable<Week>> GetAllWeeksForYearAsync(int seasonYear);

        public Task<Week> GetWeekByWeekNumberAsync(int seasonYear, int weekNumber);

        public Task<Week> GetWeekByRefAsync(RefDto weekRef);
        public Task<int> GetWeekNumberAsync();
    }

    public class WeeksService : IWeeksService
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;

        public WeeksService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<Week>> GetAllWeeksForYearAsync(int seasonYear)
        {
            var topLevelUrl = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks?lang=en&region=us";

            var weekResponse = await _httpClient.GetFromJsonResilientAsync<WeeksResponseDto>(topLevelUrl)
                ?? throw new Exception($"Error fetching weeks for {seasonYear}");

            var weeks = new List<Week>();

            using var semaphore = new SemaphoreSlim(4);

            var tasks = weekResponse.WeekRefs.Select(async weekRef =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var week = await GetWeekByRefAsync(weekRef);
                    lock (weeks) weeks.Add(week);
                }
                finally { semaphore.Release(); }
            });

            await Task.WhenAll(tasks);
            return weeks.OrderBy(w => w.WeekNumber);
        }

        public async Task<Week> GetWeekByWeekNumberAsync(int seasonYear, int weekNumber)
        {
            var url = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks/{weekNumber}?lang=en&region=us";

            var response = await _httpClient.GetFromJsonResilientAsync<WeekDto>(url)
                ?? throw new Exception($"Week data not found for week {weekNumber} of the {seasonYear} season");

            // Resolve IEventService lazily only when needed
            var eventService = _serviceProvider.GetRequiredService<IEventService>();
            var events = await eventService.GetEventsByRefAsync(response.EventRefs);

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
            var eventService = _serviceProvider.GetRequiredService<IEventService>();
            var events = await eventService.GetEventsByRefAsync(weekResponse.EventRefs);

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