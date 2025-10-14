using backend.Models;
using backend.DTOs;
using backend.Utilities;

// TODO: Change eventDto to Events when more details are configured
namespace backend.Services
{
    public interface IEventService
    {
        public Task<IEnumerable<Event>> GetEventsByWeek(int seasonYear, int weekNumber);
        public Task<IEnumerable<Event>> GetEventsByRefAsync(RefDto eventsRef);
        public Task<Event> GetEventByRefAsync(RefDto eventRef);
        public Task<IEnumerable<Event>> GetEventsForCurrentWeek(int seasonYear);
    }

    public class EventService : IEventService
    {
        private readonly HttpClient _httpClient;
        private readonly ITeamService _teamService;
        private readonly IScoreService _scoreService;
        private readonly IOddsService _oddsService;
        private readonly IPredictorsService _predictorService;
        private readonly IWeeksService _weeksService;

        private readonly int Year = DateTime.Now.Year;

        public EventService(
            HttpClient httpClient,
            ITeamService teamService,
            IScoreService scoreService,
            IOddsService oddsService,
            IPredictorsService predictorsService,
            IWeeksService weeksService)
        {
            _httpClient = httpClient;
            _teamService = teamService;
            _scoreService = scoreService;
            _oddsService = oddsService;
            _predictorService = predictorsService;
            _weeksService = weeksService;
        }

        private async Task<(EventsResponseDto, string, string)> GetEventsResponseAsync(int seasonYear, int weekNumber)
        {
            var url = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{seasonYear}/types/2/weeks/{weekNumber}/events?lang=en&region=us";

            var response = await _httpClient.GetFromJsonResilientAsync<EventsResponseDto>(url)
                ?? throw new Exception($"Error fetching events response for week {weekNumber} of the {seasonYear} season");

            // This is a really stupid way of doing this, but I'm not sure how to get the data from the eventResponseDto to the event in any other way since we aren't storing any of this data.
            return (response, response.EventMetadata.EventParametersDto.SeasonString.FirstOrDefault(), response.EventMetadata.EventParametersDto.WeekString.FirstOrDefault());
        }

        // TODO: Look into changing this?? I think we call get event response when we already have a response?? We could pass this in from the higher level method??
        public async Task<IEnumerable<Event>> GetEventsByWeek(int seasonYear, int weekNumber)
        {
            var eventResponse = await GetEventsResponseAsync(seasonYear, weekNumber);
            var eventRefs = eventResponse.Item1;
            var season = eventResponse.Item2;
            var week = eventResponse.Item3;

            var eventList = new List<Event>();

            // TODO: Again, there is a lot of data we need to fetch from refs here before we can actually use this
            foreach (var eventRef in eventRefs.EventRefs)
            {
                var response = await _httpClient.GetFromJsonResilientAsync<EventDto>(eventRef.Ref)
                    ?? throw new Exception("Error fetching event");

                eventList.Add(await GetEventFromEventDto(response));
            }
            return eventList;
        }

        public async Task<IEnumerable<Event>> GetEventsByRefAsync(RefDto eventsRef)
        {
            var eventResposne = await _httpClient.GetFromJsonResilientAsync<EventsResponseDto>(eventsRef.Ref)
                ?? throw new Exception("Error fetching Events from Reference");

            int weekNumber = Convert.ToInt32(eventResposne.EventMetadata.EventParametersDto.WeekString.FirstOrDefault());
            int seasonNumber = Convert.ToInt32(eventResposne.EventMetadata.EventParametersDto.SeasonString.FirstOrDefault());

            var eventList = await GetEventsByWeek(seasonNumber, weekNumber);
            return eventList;
        }

        public async Task<Event> GetEventByRefAsync(RefDto eventRef)
        {
            var eventResponse = await _httpClient.GetFromJsonResilientAsync<EventDto>(eventRef.Ref)
                ?? throw new Exception("Error fetching event by ref");

            return await GetEventFromEventDto(eventResponse);
        }

        public async Task<IEnumerable<Event>> GetEventsForCurrentWeek(int seasonYear)
        {
            var weekNumber = await _weeksService.GetWeekNumberAsync();
            var week = await _weeksService.GetWeekByWeekNumberAsync(Year, weekNumber);
            var events = week.Events;
            return events;
        }

        private async Task<Event> GetEventFromEventDto(EventDto response, int seasonNumber = 2025, int weekNumber = 1)
        {
            var competitions = new List<Competition>();
            var semaphore = new SemaphoreSlim(5); 
            var tasks = response.Competitions.Select(async comp =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var competitors = await Task.WhenAll(comp.Competitors.Select(async c =>
                    {
                        return new Competitor
                        {
                            Id = c.Id,
                            HomeAway = c.HomeAway,
                            Winner = c.Winner,
                            Team = await _teamService.GetTeamAsync(c.TeamRef),
                            CompetitorScore = await _scoreService.GetTeamScoreAsync(c.ScoreRef)
                        };
                    }));

                    return new Competition
                    {
                        Id = comp.Id,
                        Date = DateTime.Parse(comp.Date, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                        TimeValid = comp.TimeValid,
                        DateValid = comp.DateValid,
                        NuetralSite = comp.NuetralSite,
                        DivisionCompetition = comp.DivisionCompetition,
                        ConferenceCompetition = comp.ConferenceCompetition,
                        Competitors = competitors.ToList(),
                        CompetitionOdds = await _oddsService.GetOddsAsync(comp.OddsRef),
                        CompetitionPredictors = await _predictorService.GetPredictionsAsync(comp.PredictorRef)
                    };
                }
                finally
                {
                    semaphore.Release();
                }
            });

            competitions.AddRange(await Task.WhenAll(tasks));

            return new Event
            {
                Id = response.Id,
                Date = DateTime.Parse(response.Date, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                Name = response.Name,
                TimeValid = response.TimeValid,
                Competitions = competitions,
                Season = seasonNumber,
                Week = weekNumber
            };
        }

    }
} 