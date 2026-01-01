using System.Threading.Tasks;
using backend.DTOs;
using backend.Models;
using backend.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Services
{
    public interface IGameDataService
    {
        public Task<IEnumerable<GameData>> GetGameDataForCurrentWeekAsync(int weekNumber);
        public Task<GameData> GetGameDataByEventId(string eventId);
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
            // This is a stupid way to fix this but since it just turned to 2026, this breaks things
            var currentYear = DateTime.Now.Year - 1;

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

        public async Task<GameData> GetGameDataByEventId(string eventId)
        {
            var game = await _eventService.GetEventByIdAsync(eventId);
            return GetGameDataFromEvent(game);
        }

        public async Task<int> GetCurrentWeekAsync()
        {
            var currentWeekNumber = await _weeksService.GetWeekNumberAsync();
            return currentWeekNumber;
        }

        private static GameData GetGameDataFromEvent(Event e)
        {
            var comp = e.Competitions.FirstOrDefault();
            var homeTeam = comp.Competitors.FirstOrDefault(team => team.HomeAway == "home");
            var awayTeam = comp.Competitors.FirstOrDefault(team => team.HomeAway == "away");

            return new GameData
            {
                HomeTeamName = homeTeam.Team.DisplayName,
                HomeTeamId = homeTeam.Team.Id,
                AwayTeamId = awayTeam.Team.Id,
                AwayTeamName = awayTeam.Team.DisplayName,
                EventId = e.Id,
                Date = e.Date,
            };
        }
    }
}