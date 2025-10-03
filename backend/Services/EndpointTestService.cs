using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.OpenApi.Validations;

namespace backend.Services
{
    public interface IEndpointTestService
    {
        public Task<Season?> TestSeasonEndpointAysnc();

        public Task<IEnumerable<Week?>> TestWeekEndpointAsync();

        public Task<IEnumerable<Event?>> TestEventsEndpointAsync();

        public Task<Odds?> TestOddsEndpointAsync();

        public Task<Event?> GetEventDataTestAsync();
        public Task<Team?> GetTeamDataTestAsync();

    }
    public class EndpointTestService : IEndpointTestService
    {
        private readonly IEventService _eventService;
        private readonly IOddsService _oddsService;
        private readonly IPredictorsService _predictorService;
        private readonly IScoreService _scoreService;
        private readonly ISeasonService _seasonService;
        private readonly ITeamService _teamService;
        private readonly IWeeksService _weeksService;

        public EndpointTestService(
            IEventService eventService,
            IOddsService oddsService,
            IPredictorsService predictorsService,
            IScoreService scoreService,
            ISeasonService seasonService,
            ITeamService teamService,
            IWeeksService weeksService)
        {
            _eventService = eventService;
            _oddsService = oddsService;
            _predictorService = predictorsService;
            _scoreService = scoreService;
            _seasonService = seasonService;
            _teamService = teamService;
            _weeksService = weeksService;
        }

        public const int seasonYear = 2025;

        public async Task<Season?> TestSeasonEndpointAysnc()
        {
            var seasonResponse = await _seasonService.GetSeasonByYearAsync(seasonYear)
                ?? throw new Exception("Error fetching Season");

            return seasonResponse;
        }

        public async Task<IEnumerable<Week?>> TestWeekEndpointAsync()
        {
            var weekResponse = await _weeksService.GetAllWeeksForYearAsync(seasonYear)
                ?? throw new Exception("Error fetching weeks");

            return weekResponse;
        }

        public async Task<IEnumerable<Event?>> TestEventsEndpointAsync()
        {
            int weekNumber = 2;
            var eventResponse = await _eventService.GetEventsByWeek(seasonYear, weekNumber)
                ?? throw new Exception("Error fetching events");

            return eventResponse;
        }

        public async Task<Odds?> TestOddsEndpointAsync()
        {
            var oddsRef = new RefDto
            {
                Ref = "http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/events/401772938/competitions/401772938/odds?lang=en&region=us"
            };

            var oddsResponse = await _oddsService.GetOddsAsync(oddsRef);
            return oddsResponse;
        }

        public async Task<Event?> GetEventDataTestAsync()
        {
            var eventRef = new RefDto
            {
                Ref = "http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/events/401772939?lang=en&region=us"
            };

            var eventResponse = await _eventService.GetEventByRefAsync(eventRef);
            return eventResponse;
        }

        public async Task<Team?> GetTeamDataTestAsync()
        {
            var teamRef = new RefDto
            {
                Ref = "http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/2025/teams/14?lang=en&region=us"
            };

            var teamResponse = await _teamService.GetTeamAsync(teamRef);
            return teamResponse;
        }
    }
}