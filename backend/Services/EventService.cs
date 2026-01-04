using backend.Models;
using backend.DTOs;
using backend.Utilities;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Services
{
    public interface IEventService
    {
        public Task<IEnumerable<Event>> GetEventsByWeek(int seasonYear, int weekNumber);
        public Task<IEnumerable<Event>> GetEventsByRefAsync(RefDto eventsRef);
        public Task<Event> GetEventByRefAsync(RefDto eventRef);
        public Task<IEnumerable<Event>> GetEventsForCurrentWeek(int seasonYear);
        public Task<IEnumerable<GameData>> GetGameDataByEventRef(RefDto eventRef, int seasonNumber);
        public Task<Event> GetEventByIdAsync(string eventId);
    }

    public class EventService : IEventService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ITeamService _teamService;
        private readonly IScoreService _scoreService;
        private readonly IOddsService _oddsService;
        private readonly IPredictorsService _predictorService;
        private readonly IWeeksService _weeksService;
        private readonly IWeatherService _weatherService;
        private readonly int Year = DateTime.Now.Year - 1;

        public EventService(
            HttpClient httpClient,
            IMemoryCache cache,
            ITeamService teamService,
            IScoreService scoreService,
            IOddsService oddsService,
            IPredictorsService predictorsService,
            IWeeksService weeksService,
            IWeatherService weatherService)
        {
            _httpClient = httpClient;
            _cache = cache;
            _teamService = teamService;
            _scoreService = scoreService;
            _oddsService = oddsService;
            _predictorService = predictorsService;
            _weeksService = weeksService;
            _weatherService = weatherService;
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
            var week = await _weeksService.GetWeekDataByWeekNumberAsync(Year, weekNumber);
            var events = week.Events;
            return events;
        }

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

                var date = DateTime.Parse(comp.Date, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

                var competitorsTask = Task.WhenAll(competitorTasks);
                var oddsTask = _oddsService.GetOddsAsync(comp.OddsRef);
                var predictorsTask = _predictorService.GetPredictionsAsync(comp.PredictorRef);
                var weatherTask = _weatherService.GetWeatherForCompetitionAsync(comp.Venue.AddressDto.City, comp.Venue.AddressDto.Country, date);

                await Task.WhenAll(competitorsTask, oddsTask, predictorsTask, weatherTask);

                return new Competition
                {
                    Id = comp.Id,
                    Date = date,
                    TimeValid = comp.TimeValid,
                    DateValid = comp.DateValid,
                    Venue = new Venue
                    {
                        StadiumName = comp.Venue.StadiumName,
                        City = comp.Venue.AddressDto.City,
                        State = comp.Venue.AddressDto.State,
                        ZipCode = comp.Venue.AddressDto.zipCode,
                        Country = comp.Venue.AddressDto.Country,
                        Grass = comp.Venue.Grass,
                        Indoors = comp.Venue.Indoors,
                        OutOfUSA = comp.Venue.AddressDto.Country != "USA"
                    },
                    Weather = await weatherTask,
                    Competitors = (await competitorsTask).ToList(),
                    CompetitionOdds = (await oddsTask).ToList(),
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

        public async Task<Event> GetEventByIdAsync(string eventId)
        {
            var eventRef = new RefDto
            {
                Ref = $"http://sports.core.api.espn.com/v2/sports/football/leagues/nfl/events/{eventId}?lang=en&region=us"
            };

            return await GetEventByRefAsync(eventRef);
        }

        public async Task<IEnumerable<GameData>> GetGameDataByEventRef(RefDto eventRef, int seasonNumber)
        {
            var eventResponse = await _httpClient.GetFromJsonResilientAsync<EventDto>(eventRef.Ref)
                ?? throw new Exception("Error fetching event by ref");

            var cacheKey = $"gamedata:{eventResponse.Id}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<GameData> cachedGameData))
            {
                return cachedGameData;
            }

            var gameDataTasks = eventResponse.Competitions.Select(async comp =>
            {
                var competitorTasks = comp.Competitors.Select(async c =>
                {
                    var team = await _teamService.GetTeamAsync(c.TeamRef, seasonNumber);

                    return new Competitor
                    {
                        Id = c.Id,
                        HomeAway = c.HomeAway,
                        Winner = c.Winner,
                        Team = team,
                        CompetitorScore = new Score{}
                    };
                });

                var competitors = await Task.WhenAll(competitorTasks);
                var date = DateTime.Parse(comp.Date, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

                var homeTeam = competitors.FirstOrDefault(c => c.HomeAway == "home")?.Team;
                var awayTeam = competitors.FirstOrDefault(c => c.HomeAway == "away")?.Team;

                if (homeTeam == null || awayTeam == null)
                {
                    throw new Exception($"Missing home or away team for competition {comp.Id}");
                }

                return new GameData
                {
                    HomeTeamName = homeTeam.Name,
                    AwayTeamName = awayTeam.Name,
                    HomeTeamId = homeTeam.Id,
                    AwayTeamId = awayTeam.Id,
                    EventId = eventResponse.Id,
                    Date = date
                };
            });

            var gameDataResponse = await Task.WhenAll(gameDataTasks);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(6));

            _cache.Set(cacheKey, gameDataResponse, cacheOptions);
            
            return gameDataResponse;
        }
    }
} 



