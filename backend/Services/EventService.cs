using backend.Models;
using backend.DTOs;

// TODO: Change eventDto to Events when more details are configured
namespace backend.Services
{
    public interface IEventService
    {
        public Task<IEnumerable<Event>> GetEventsByWeek(int seasonYear, int weekNumber);
        public Task<IEnumerable<Event>> GetEventsByRefAsync(RefDto eventsRef);
    }

    public class EventService : IEventService
    {
        private readonly HttpClient _httpClient;
        private readonly ITeamService _teamService;
        private readonly IScoreService _scoreService;
        private readonly IOddsService _oddsService;
        private readonly IPredictorsService _predictorService;

        public EventService(
            HttpClient httpClient,
            ITeamService teamService,
            IScoreService scoreService,
            IOddsService oddsService,
            IPredictorsService predictorsService)
        {
            _httpClient = httpClient;
            _teamService = teamService;
            _scoreService = scoreService;
            _oddsService = oddsService;
            _predictorService = predictorsService;
        }

        private async Task<EventsResponseDto> GetEventsResponseAsync(int seasonYear, int weekNumber)
        {
            var url = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks/{weekNumber}/events?lang=en&region=us";

            var response = await _httpClient.GetFromJsonAsync<EventsResponseDto>(url)
                ?? throw new Exception($"Error fetching events response for week {weekNumber} of the {seasonYear} season");

            return response;
        }

        public async Task<IEnumerable<Event>> GetEventsByWeek(int seasonYear, int weekNumber)
        {
            var eventResponse = await GetEventsResponseAsync(seasonYear, weekNumber);

            var eventList = new List<Event>();

            // TODO: Again, there is a lot of data we need to fetch from refs here before we can actually use this
            foreach (var eventRef in eventResponse.EventRefs)
            {
                var response = await _httpClient.GetFromJsonAsync<EventDto>(eventRef.Ref)
                    ?? throw new Exception("Error fetching event");

                eventList.Add(new Event
                {
                    Id = response.Id,
                    Date = DateTime.Parse(response.Date, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                    Name = response.Name,
                    TimeValid = response.TimeValid,
                    Competitions = (await Task.WhenAll(
                        response.Competitions.Select(async comp => new Competition
                        {
                            Id = comp.Id,
                            Date = DateTime.Parse(comp.Date, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                            TimeValid = comp.TimeValid,
                            DateValid = comp.DateValid,
                            NuetralSite = comp.NuetralSite,
                            DivisionCompetition = comp.DivisionCompetition,
                            ConferenceCompetition = comp.ConferenceCompetition,
                            Competitors = (await Task.WhenAll(
                                comp.Competitors.Select(async c => new Competitor
                                {
                                    Id = c.Id,
                                    HomeAway = c.HomeAway,
                                    Winner = c.Winner,
                                    Team = await _teamService.GetTeamAsync(c.TeamRef),
                                    CompetitorScore = await _scoreService.GetTeamScoreAsync(c.ScoreRef)
                                })
                            )).ToList(),

                            CompetitionOdds = await _oddsService.GetOddsAsync(comp.OddsRef),
                            CompetitionPredictors = await _predictorService.GetPredictionsAsync(comp.PredictorRef)
                        })
                    )).ToList(),
                    Season = Convert.ToInt32(response.Season),
                    Week = Convert.ToInt32(response.Week)
                });
            }
            return eventList;
        }

        public async Task<IEnumerable<Event>> GetEventsByRefAsync(RefDto eventsRef)
        {
            var eventResposne = await _httpClient.GetFromJsonAsync<EventsResponseDto>(eventsRef.Ref)
                ?? throw new Exception("Error fetching Events from Reference");

            int weekNumber = Convert.ToInt32(eventResposne.EventMetadata.EventParametersDto.WeekString.FirstOrDefault());
            int seasonNumber = Convert.ToInt32(eventResposne.EventMetadata.EventParametersDto.SeasonString.FirstOrDefault());

            var eventList = await GetEventsByWeek(seasonNumber, weekNumber);
            return eventList;
        }
    }
} 