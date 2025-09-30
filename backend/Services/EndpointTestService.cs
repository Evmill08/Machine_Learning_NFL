using System.Net.Security;
using backend.Models;
using backend.Services;

namespace backend.Services
{
    public interface IEndpointTestService
    {
        public Task TestSeasonEndpointAysnc();

        public Task TestWeekEndpointAsync();

        public Task TestEventsEndpointAsync();

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

        public async Task TestSeasonEndpointAysnc()
        {
            var seasonResponse = await _seasonService.GetSeasonByYearAsync(seasonYear)
                ?? throw new Exception("Error fetching Season");
        }

        public async Task TestWeekEndpointAsync()
        {
            var weekResponse = await _weeksService.GetAllWeeksForYearAsync(seasonYear)
                ?? throw new Exception("Error fetching weeks");
        }

        public async Task TestEventsEndpointAsync()
        {
            int weekNumber = 2;
            var eventResponse = await _eventService.GetEventsByWeek(seasonYear, weekNumber)
                ?? throw new Exception("Error fetching events");
        }





    }
}