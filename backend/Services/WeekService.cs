using System.Security.Principal;
using backend.DTOs;
using backend.Models;

// TODO:
// Again, we need to think about the transferring from weekDto to Week Model
namespace backend.Services
{
    public interface IWeeksService
    {
        public Task<IEnumerable<Week>> GetAllWeeksForYearAsync(int seasonYear);

        public Task<Week> GetWeekByWeekNumberAsync(int seasonYear, int weekNumber);

        public Task<Week> GetWeekByRefAsync(RefDto weekRef);
    }

    public class WeeksService : IWeeksService
    {
        private readonly HttpClient _httpClient;
        private readonly IEventService _eventService;

        public WeeksService(HttpClient httpClient, IEventService eventService)
        {
            _httpClient = httpClient;
            _eventService = eventService;
        }

        public async Task<IEnumerable<Week>> GetAllWeeksForYearAsync(int seasonYear)
        {
            var topLevelUrl = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks?lang=en&region=us";

            var weekResponse = await _httpClient.GetFromJsonAsync<WeeksResponseDto>(topLevelUrl)
                ?? throw new Exception($"Error fetching weeks for {seasonYear}");

            var weeks = new List<Week>();

            using var semaphore = new SemaphoreSlim(5);

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

            var response = await _httpClient.GetFromJsonAsync<WeekDto>(url)
                ?? throw new Exception($"Week data not found for week {weekNumber} of the {seasonYear} season");

            var events = await _eventService.GetEventsByRefAsync(response.EventRefs);

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
            var weekResponse = await _httpClient.GetFromJsonAsync<WeekDto>(weekRef.Ref)
                ?? throw new Exception("Error fecthing week by week reference");

            var events = await _eventService.GetEventsByRefAsync(weekResponse.EventRefs);

            return new Week
            {
                WeekNumber = weekResponse.Number,
                StartDate = DateTime.Parse(weekResponse.StartDate, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                EndDate = DateTime.Parse(weekResponse.EndDate, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                Events = [.. events]
            };
        }
    }
}