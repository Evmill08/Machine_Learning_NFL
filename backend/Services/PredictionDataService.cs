using System.Collections.Concurrent;
using backend.enums;
using backend.Models;

// TODO: Fully optimize this service
namespace backend.Services
{
    public interface IPredictionDataService
    {
        public Task<IEnumerable<PredictionData>> GetPredictionDataForTimeframe(int startYear, int EndYear);
        public Task<IEnumerable<PredictionData>> GetPredictionDataForYear(int year);
        public Task<PredictionData> GetPredictionDataForEvent(Event game);
    }

    public class PredictionDataService : IPredictionDataService
    {
        private readonly ISeasonService _seasonService;
        private readonly IOddsService _oddsService;
        private readonly int _maxDegreesOfParallelism;

        public PredictionDataService(ISeasonService seasonService, IOddsService oddsService)
        {
            _seasonService = seasonService;
            _oddsService = oddsService;
            _maxDegreesOfParallelism = Math.Max(1, Environment.ProcessorCount);
        }

        public async Task<IEnumerable<PredictionData>> GetPredictionDataForTimeframe(int startYear, int endYear)
        {
            var seasons = await _seasonService.GetSeasonsRangedAsync(startYear, endYear);

            var allEvents = seasons
                .Where(s => s.SeasonType?.Weeks != null)
                .SelectMany(s => s.SeasonType!.Weeks!)
                .SelectMany(w => w.Events ?? Enumerable.Empty<Event>())
                .ToList();

            var predictionData = new ConcurrentBag<PredictionData>();

            var options = new ParallelOptions { MaxDegreeOfParallelism = _maxDegreesOfParallelism };

            await Parallel.ForEachAsync(allEvents, options, async (game, ct) =>
            {
                var data = await GetPredictionDataForEvent(game);
                predictionData.Add(data);
            });

            return predictionData.OrderBy(pd => pd.SeasonYear).ThenBy(pd => pd.WeekNumber).ToList();
        }

        public async Task<IEnumerable<PredictionData>> GetPredictionDataForYear(int year)
        {
            var season = await _seasonService.GetSeasonByYearAsync(year);
            var predictionData = new ConcurrentBag<PredictionData>();

            var weeks = season.SeasonType.Weeks;

            Parallel.ForEach(
                weeks,
                new ParallelOptions { MaxDegreeOfParallelism = _maxDegreesOfParallelism },
                async week =>
                {
                    foreach (var e in week.Events ?? Enumerable.Empty<Event>())
                    {
                        var data = await GetPredictionDataForEvent(e);
                        predictionData.Add(data);
                    }
                });

            return predictionData.OrderBy(pd => pd.WeekNumber).ToList();
        }


        public async Task<PredictionData> GetPredictionDataForEvent(Event game)
        {
            var competition = game.Competitions.FirstOrDefault();
            var homeTeam = competition.Competitors.FirstOrDefault(team => team.HomeAway == "home");
            var awayTeam = competition.Competitors.FirstOrDefault(team => team.HomeAway == "away");

            var homeCompPreds = competition.CompetitionPredictors.HomeTeamPredictors.Predictors;
            var awayCompPreds = competition.CompetitionPredictors.AwayTeamPredictors.Predictors;

            var totalPoints = homeTeam.CompetitorScore.Value + awayTeam.CompetitorScore.Value;
            var homeWin = homeTeam.CompetitorScore.Winner ? 1 : 0;
            var spread = homeTeam.CompetitorScore.Value - awayTeam.CompetitorScore.Value;

            var (bestTotal, bestSpread) = await _oddsService.GetBestOdds(game);
            var allOdds = competition.CompetitionOdds;

            var averagePredictedTotal = allOdds.Average(o => o.OverUnder);
            var averagePredictedSpread = allOdds.Average(o => o.Spread);

            return new PredictionData
            {
                TotalPoints = (int)totalPoints,
                Spread = (int)spread,
                HomeWin = homeWin,
                SeasonYear = game.Season,
                WeekNumber = game.Week,
                HomeTeamName = homeTeam.Team.Name,
                AwayTeamName = awayTeam.Team.Name,
                HomeTeamId = homeTeam.Team.Id,
                AwayTeamId = awayTeam.Team.Id,
                EventId = game.Id,
                HomeWinner = homeTeam.Winner ? 1 : 0,
                AveragePredictedTotal = averagePredictedTotal,
                AveragePredictedSpread = averagePredictedSpread,
                BestPredictedSpread = bestSpread.OddsValue,
                BestPredictedTotal = bestTotal.OddsValue,
                MeanTemperature = competition.Weather.MeanTemperature,
                MaxTemperature = competition.Weather.MaxTemperature,
                MinTemperature = competition.Weather.MinTemperature,
                ApparentTemperature = competition.Weather.ApparentTemperature,
                PrecipitationSum = competition.Weather.PrecipitationSum,
                SnowfallSum = competition.Weather.SnowfallSum,
                PrecipitationHours = competition.Weather.PrecipitationHours,
                RainSum = competition.Weather.RainSum,
                WindSpeedMax = competition.Weather.WindSpeedMax,
                WindGustsMax = competition.Weather.WindGustsMax,
                DominantWindDirection = competition.Weather.DominantWindDirection,
                MeanRelativeHumidity = competition.Weather.MeanRelativeHumidity,
                MeanWindGusts = competition.Weather.MeanWindGusts,
                MeanWindSpeed = competition.Weather.MeanWindSpeed,
                PredictedHomeWinPercent = GetPredictorValue(homeCompPreds, CompetitionPredictors.WinPercent),
                PredicitedHomeMatchupQuality = GetPredictorValue(homeCompPreds, CompetitionPredictors.MatchupQuality),
                PredictedHomeLossPercent = GetPredictorValue(homeCompPreds, CompetitionPredictors.LossPercent),
                PredictedHomePointDifferential = GetPredictorValue(homeCompPreds, CompetitionPredictors.PointDifferential),
                PredictedHomeDefenseEfficiency = GetPredictorValue(homeCompPreds, CompetitionPredictors.DefenseEfficiency),
                PredictedHomeOffenseEfficiency = GetPredictorValue(homeCompPreds, CompetitionPredictors.OffenseEfficiency),
                PredictedHomeTotalEfficiency = GetPredictorValue(homeCompPreds, CompetitionPredictors.TotalEfficiency),
                PredictedAwayWinPercent = GetPredictorValue(awayCompPreds, CompetitionPredictors.WinPercent),
                PredicitedAwayMatchupQuality = GetPredictorValue(awayCompPreds, CompetitionPredictors.MatchupQuality),
                PredictedAwayLossPercent = GetPredictorValue(awayCompPreds, CompetitionPredictors.LossPercent),
                PredictedAwayDefenseEfficiency = GetPredictorValue(awayCompPreds, CompetitionPredictors.DefenseEfficiency),
                PredictedAwayOffenseEfficiency = GetPredictorValue(awayCompPreds, CompetitionPredictors.OffenseEfficiency),
                PredictedAwayTotalEfficiency = GetPredictorValue(awayCompPreds, CompetitionPredictors.TotalEfficiency),
                PredictedAwayPointDifferential = GetPredictorValue(awayCompPreds, CompetitionPredictors.PointDifferential),
            };
        }

        private static decimal GetPredictorValue(IEnumerable<Statistic> compStats, string name)
        {
            if (compStats == null)
                return 0;

            var compStat = compStats.FirstOrDefault(s => s.Name == name);
            if (compStat == null)
                return 0;

            return compStat.Value;
        }
    }
}