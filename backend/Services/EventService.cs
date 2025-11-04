using backend.Models;
using backend.DTOs;
using backend.Utilities;
using DocumentFormat.OpenXml.ExtendedProperties;

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
            return (

                Response: response,
                Season: response.EventMetadata.EventParametersDto.SeasonString.FirstOrDefault(),
                Week: response.EventMetadata.EventParametersDto.WeekString.FirstOrDefault()
            );
        }

        // TODO: Look into changing this?? I think we call get event response when we already have a response?? We could pass this in from the higher level method??
        public async Task<IEnumerable<Event>> GetEventsByWeek(int seasonYear, int weekNumber)
        {
            var (response, season, week) = await GetEventsResponseAsync(seasonYear, weekNumber);

            var eventTasks = response.EventRefs.Select(async eventRef =>
            {
                var response = await _httpClient.GetFromJsonResilientAsync<EventDto>(eventRef.Ref)
                    ?? throw new Exception("Error fecthing event");

                return await GetEventFromEventDto(response, Convert.ToInt32(season), Convert.ToInt32(week));
            });

            var eventList = await Task.WhenAll(eventTasks);
            return eventList;
        }

        public async Task<IEnumerable<Event>> GetEventsByRefAsync(RefDto eventsRef)
        {
            var eventResposne = await _httpClient.GetFromJsonResilientAsync<EventsResponseDto>(eventsRef.Ref)
                ?? throw new Exception("Error fetching Events from Reference");

            int weekNumber = Convert.ToInt32(eventResposne.EventMetadata.EventParametersDto.WeekString.FirstOrDefault());
            int seasonNumber = Convert.ToInt32(eventResposne.EventMetadata.EventParametersDto.SeasonString.FirstOrDefault());

            var eventTasks = eventResposne.EventRefs.Select(async eventRef =>
            {
                var response = await _httpClient.GetFromJsonResilientAsync<EventDto>(eventRef.Ref)
                    ?? throw new Exception("Error fetching event");

                return await GetEventFromEventDto(response, seasonNumber, weekNumber);
            });

            return await Task.WhenAll(eventTasks);
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

        // Change this bs
        private async Task<Event> GetEventFromEventDto(EventDto response, int seasonNumber = 2025, int weekNumber = 1)
        {
            var competitionTasks = response.Competitions.Select(async comp =>
            {
                var competitorTasks = comp.Competitors.Select(async c =>
                {
                    var teamTask = _teamService.GetTeamAsync(c.TeamRef, seasonNumber);
                    var scoreTask = _scoreService.GetTeamScoreAsync(c.ScoreRef);

                    await Task.WhenAll(teamTask, scoreTask);

                    return new Competitor
                    {
                        Id = c.Id,
                        HomeAway = c.HomeAway,
                        Winner = c.Winner,
                        Team = await teamTask,
                        CompetitorScore = await scoreTask
                    };
                });

                var competitorsTask = Task.WhenAll(competitorTasks);
                var oddsTask = _oddsService.GetOddsAsync(comp.OddsRef);
                var predictorsTask = _predictorService.GetPredictionsAsync(comp.PredictorRef);

                await Task.WhenAll(competitorsTask, oddsTask, predictorsTask);

                return new Competition
                {
                    Id = comp.Id,
                    Date = DateTime.Parse(comp.Date, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                    TimeValid = comp.TimeValid,
                    DateValid = comp.DateValid,
                    NuetralSite = comp.NuetralSite,
                    DivisionCompetition = comp.DivisionCompetition,
                    ConferenceCompetition = comp.ConferenceCompetition,
                    Competitors = (await competitorsTask).ToList(),
                    CompetitionOdds = await oddsTask,
                    CompetitionPredictors = await predictorsTask,
                };
            });

            var competitions = await Task.WhenAll(competitionTasks);

            return new Event
            {
                Id = response.Id,
                Date = DateTime.Parse(response.Date, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
                Name = response.Name,
                TimeValid = response.TimeValid,
                Competitions = competitions.ToList(),
                Season = seasonNumber,
                Week = weekNumber
            };
        }
    }
} 



