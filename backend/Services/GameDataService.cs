using backend.Models;

namespace backend.Services
{
    public interface IGameDataService
    {
        public Task<IEnumerable<GameData>> GetGameDataForCurrentWeekAsync();
    }

    public class GameDataService : IGameDataService
    {
        private readonly IWeeksService _weeksService;

        public GameDataService(IWeeksService weeksService)
        {
            _weeksService = weeksService;
        }

        public async Task<IEnumerable<GameData>> GetGameDataForCurrentWeekAsync()
        {
            var currentWeekNumber = await _weeksService.GetWeekNumberAsync();
            var currentYear = DateTime.Now.Year;

            var currentWeek = await _weeksService.GetWeekByWeekNumberAsync(currentYear, currentWeekNumber);

            var currentWeekGames = new List<GameData>();

            Parallel.ForEach(
                currentWeek.Events,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                async e =>
                {
                    var competition = e.Competitions.FirstOrDefault();
                    var homeTeam = competition.Competitors.FirstOrDefault(team => team.HomeAway == "home").Team;
                    var awayTeam = competition.Competitors.FirstOrDefault(team => team.HomeAway == "away").Team;

                    currentWeekGames.Add(new GameData
                    {
                        HomeTeamName = homeTeam.Name,
                        AwayTeamName = awayTeam.Name,
                        HomeTeamID = homeTeam.Id,
                        AwayTeamId = awayTeam.Id,
                        EventId = e.Id,
                        Date = competition.Date,
                    });
                }
            );

            return currentWeekGames;
        }
    }
}