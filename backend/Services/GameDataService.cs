using backend.DTOs;
using backend.Models;
using backend.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Services
{
    public interface IGameDataService
    {
        public Task<IEnumerable<GameData>> GetGameDataForCurrentWeekAsync(int weekNumber);
        public Task<int> GetCurrentWeekAsync();
    }

    public class GameDataService : IGameDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IWeeksService _weeksService;
        private readonly IEventService _eventService;

        public GameDataService(HttpClient httpClient, IWeeksService weeksService, IEventService eventService)
        {
            _httpClient = httpClient;
            _weeksService = weeksService;
            _eventService = eventService;
        }

        public async Task<IEnumerable<GameData>> GetGameDataForCurrentWeekAsync(int weekNumber)
        {
            var currentYear = DateTime.Now.Year;

            var currentWeek = await _weeksService.GetWeekByWeekNumberAsync(currentYear, weekNumber);

            var eventsResponse = await _httpClient.GetFromJsonResilientAsync<EventsResponseDto>(currentWeek.EventRefs.Ref)
                ?? throw new Exception("Error fetching events for current week");

            var gameDataTasks = eventsResponse.EventRefs.Select(async eventRef =>
            {
                return await _eventService.GetGameDataByEventRef(eventRef, currentYear);
            });

            var gameDataResults = await Task.WhenAll(gameDataTasks);
            return gameDataResults.SelectMany(g => g);
        }

        public async Task<int> GetCurrentWeekAsync()
        {
            var currentWeekNumber = await _weeksService.GetWeekNumberAsync();
            return currentWeekNumber;
        }
    }
}