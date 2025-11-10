using System.Collections.Concurrent;
using backend.enums;
using backend.Models;
using DocumentFormat.OpenXml.Drawing.Charts;

// TODO: Fully optimize this service
namespace backend.Services
{
    public interface IPredictionDataService
    {
        public Task<IEnumerable<PredictionData>> GetPredictionDataForTimeframe(int startYear, int EndYear);
        public Task<IEnumerable<PredictionData>> GetPredictionDataForYear(int year);
        public PredictionData GetPredictionDataForEvent(Event game);
    }

    public class PredictionDataService : IPredictionDataService
    {
        private readonly ISeasonService _seasonService;
        private readonly ITeamService _teamService;
        private readonly int _maxDegreesOfParallelism;

        public PredictionDataService(ISeasonService seasonService, ITeamService teamService)
        {
            _seasonService = seasonService;
            _teamService = teamService;
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

            await Parallel.ForEachAsync(allEvents, options, (game, ct) =>
            {
                var data = GetPredictionDataForEvent(game);
                predictionData.Add(data);
                return ValueTask.CompletedTask;
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
                week =>
                {
                    foreach (var e in week.Events ?? Enumerable.Empty<Event>())
                    {
                        var data = GetPredictionDataForEvent(e);
                        predictionData.Add(data);
                    }
                });

            return predictionData.OrderBy(pd => pd.WeekNumber).ToList();
        }


        public PredictionData GetPredictionDataForEvent(Event game)
        {
            var competition = game.Competitions.FirstOrDefault();
            var homeTeam = competition.Competitors.FirstOrDefault(team => team.HomeAway == "home");
            var awayTeam = competition.Competitors.FirstOrDefault(team => team.HomeAway == "away");

            var homeOddsRecord = homeTeam.Team.OddsRecord?.OddsStats ?? Enumerable.Empty<OddsStat>();
            var awayOddsRecord = awayTeam.Team.OddsRecord?.OddsStats ?? Enumerable.Empty<OddsStat>();

            // Use the cached categorized stats - MUCH FASTER!
            var homeStats = _teamService.GetCategorizedStats(homeTeam.Team, game.Season);
            var awayStats = _teamService.GetCategorizedStats(awayTeam.Team, game.Season);

            // Now use O(1) dictionary lookups instead of repeated LINQ queries
            var homePassingStats = homeStats.GetValueOrDefault(StatCategories.Passing);
            var homeRushingStats = homeStats.GetValueOrDefault(StatCategories.Rushing);
            var homeReceivingStats = homeStats.GetValueOrDefault(StatCategories.Receiving);
            var homeKickingStats = homeStats.GetValueOrDefault(StatCategories.Kicking);
            var homePuntingStats = homeStats.GetValueOrDefault(StatCategories.Punting);
            var homeDefenseStats = homeStats.GetValueOrDefault(StatCategories.Defense);
            var homeScoringStats = homeStats.GetValueOrDefault(StatCategories.Scoring);
            var homeMiscStats = homeStats.GetValueOrDefault(StatCategories.Misc);

            var awayPassingStats = awayStats.GetValueOrDefault(StatCategories.Passing);
            var awayRushingStats = awayStats.GetValueOrDefault(StatCategories.Rushing);
            var awayReceivingStats = awayStats.GetValueOrDefault(StatCategories.Receiving);
            var awayKickingStats = awayStats.GetValueOrDefault(StatCategories.Kicking);
            var awayPuntingStats = awayStats.GetValueOrDefault(StatCategories.Punting);
            var awayDefenseStats = awayStats.GetValueOrDefault(StatCategories.Defense);
            var awayScoringStats = awayStats.GetValueOrDefault(StatCategories.Scoring);
            var awayMiscStats = awayStats.GetValueOrDefault(StatCategories.Misc);

            var homeCompPreds = competition.CompetitionPredictors.HomeTeamPredictors.Predictors;
            var awayCompPreds = competition.CompetitionPredictors.AwayTeamPredictors.Predictors;

            var totalPoints = homeTeam.CompetitorScore.Value + awayTeam.CompetitorScore.Value;
            var homeWin = homeTeam.CompetitorScore.Winner ? 1 : 0;
            var spread = homeTeam.CompetitorScore.Value - awayTeam.CompetitorScore.Value;

            return new PredictionData
            {
                TotalPoints = (int)totalPoints,
                Spread = (int)spread,
                HomeWin = homeWin,
                SeasonYear = game.Season,
                WeekNumber = game.Week,
                HomeTeamName = homeTeam.Team.Name,
                AwayTeamName = awayTeam.Team.Name,
                HomeWinner = homeTeam.Winner ? 1 : 0,
                HomeWins = homeTeam.Team.Record.Wins,
                HomeLosses = homeTeam.Team.Record.Losses,
                HomeAveragePointsAgainst = homeTeam.Team.Record.AveragePointsAgainst,
                HomeAveragePointsFor = homeTeam.Team.Record.AveragePointsFor,
                HomePointDifferential = homeTeam.Team.Record.PointDifferential,
                HomeStreak = homeTeam.Team.Record.Streak,
                HomeWinPercent = homeTeam.Team.Record.WinPercent,
                HomeDivisionWins = homeTeam.Team.Record.DivisionWins,
                HomeDivisionLosses = homeTeam.Team.Record.DivisionLosses,
                HomeTeamHomeWins = homeTeam.Team.Record.HomeWins,
                HomeTeamHomeLosses = homeTeam.Team.Record.HomeLosses,
                HomeTeamAwayWins = homeTeam.Team.Record.AwayWins,
                HomeTeamAwayLosses = homeTeam.Team.Record.AwayLosses,
                HomeConferenceWins = homeTeam.Team.Record.ConferenceWins,
                HomeConferenceLosses = homeTeam.Team.Record.ConferenceLosses,
                HomeInjuryCount = homeTeam.Team.Injuries.InjuryCount,
                AwayWinner = awayTeam.Winner ? 1 : 0,
                AwayWins = awayTeam.Team.Record.Wins,
                AwayLosses = awayTeam.Team.Record.Losses,
                AwayAveragePointsAgainst = awayTeam.Team.Record.AveragePointsAgainst,
                AwayAveragePointsFor = awayTeam.Team.Record.AveragePointsFor,
                AwayPointDifferential = awayTeam.Team.Record.PointDifferential,
                AwayStreak = awayTeam.Team.Record.Streak,
                AwayWinPercent = awayTeam.Team.Record.WinPercent,
                AwayDivisionWins = awayTeam.Team.Record.DivisionWins,
                AwayDivisionLosses = awayTeam.Team.Record.DivisionLosses,
                AwayTeamHomeWins = awayTeam.Team.Record.HomeWins,
                AwayTeamHomeLosses = awayTeam.Team.Record.HomeLosses,
                AwayTeamAwayWins = awayTeam.Team.Record.AwayWins,
                AwayTeamAwayLosses = awayTeam.Team.Record.AwayLosses,
                AwayConferenceWins = awayTeam.Team.Record.ConferenceWins,
                AwayConferenceLosses = awayTeam.Team.Record.ConferenceLosses,
                AwayInjuryCount = awayTeam.Team.Injuries.InjuryCount,
                AveragePredictedTotal = competition.CompetitionOdds.AverageOverUnder,
                AveragePredictedSpread = competition.CompetitionOdds.AverageSpread,
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
                HomeQbr = GetStatValue(homePassingStats, Passing.QBR),
                HomeCompletionPercent = GetStatValue(homePassingStats, Passing.CompletionPercent),
                HomeNetPassYardsPerGame = GetStatValue(homePassingStats, Passing.NetPassYardsPerGame),
                HomePassingBigPlays = GetStatValue(homePassingStats, Passing.PassingBigPlays),
                HomeScrimmageYardsPerGame = GetStatValue(homePassingStats, Passing.ScrimmageYardsPerGame),
                HomeYardsPerPassAttempt = GetStatValue(homePassingStats, Passing.YardsPerPassAttempt),
                HomeRbRating = GetStatValue(homeRushingStats, Rushing.RbRating),
                HomeRushYardsPerGame = GetStatValue(homeRushingStats, Rushing.RushYardsPerGame),
                HomeYardsPerRushAttempt = GetStatValue(homeRushingStats, Rushing.YardsPerRushAttempt),
                HomeRushingTouchdowns = GetStatValue(homeRushingStats, Rushing.RushingTouchdowns),
                HomeWrRating = GetStatValue(homeReceivingStats, Receiving.WrRating),
                HomeReceivingTouchdowns = GetStatValue(homeReceivingStats, Receiving.ReceivingTouchdowns),
                HomeReceivingYardsPerGame = GetStatValue(homeReceivingStats, Receiving.ReceivingYardsPerGame),
                HomeReceivingYardsPerReception = GetStatValue(homeReceivingStats, Receiving.ReceivingYardsPerReception),
                HomeAverageSackYards = GetStatValue(homeDefenseStats, Defense.AverageSackYards),
                HomeAverageStuffYards = GetStatValue(homeDefenseStats, Defense.AverageStuffYards),
                HomeDefensiveTouchdowns = GetStatValue(homeDefenseStats, Defense.DefensiveTouchdowns),
                HomeSacks = GetStatValue(homeDefenseStats, Defense.Sacks),
                HomeExtraPointPercentage = GetStatValue(homeKickingStats, Kicking.ExtraPointPercentage),
                HomeFieldGoalPercent = GetStatValue(homeKickingStats, Kicking.FieldGoalPercent),
                HomeFieldGoalsMade = GetStatValue(homeKickingStats, Kicking.FieldGoalsMade),
                HomeTotalKickingPoints = GetStatValue(homeKickingStats, Kicking.TotalKickingPoints),
                HomeNetAveragePuntYards = GetStatValue(homePuntingStats, Punting.NetAveragePuntYards),
                HomeTotalPunts = GetStatValue(homePuntingStats, Punting.TotalPunts),
                HomePuntsInsideTwenty = GetStatValue(homePuntingStats, Punting.PuntsInsideTwenty),
                HomeDefensivePoints = GetStatValue(homeScoringStats, Scoring.DefensivePoints),
                HomePassingTouchdowns = GetStatValue(homeScoringStats, Scoring.PassingTouchdowns),
                HomeTotalPoints = GetStatValue(homeScoringStats, Scoring.TotalPoints),
                HomeFirstDownsPerGame = GetStatValue(homeMiscStats, Miscellaneous.FirstDownsPerGame),
                HomeFourthDownConversionPercent = GetStatValue(homeMiscStats, Miscellaneous.FourthDownConversionPercent),
                HomeThirdDownConversionPercent = GetStatValue(homeMiscStats, Miscellaneous.ThirdDownConversionPercent),
                HomeRedZoneEfficiencyPercent = GetStatValue(homeMiscStats, Miscellaneous.RedZoneEfficiencyPercent),
                HomeTurnoverDifferential = GetStatValue(homeMiscStats, Miscellaneous.TurnoverDifferential),
                AwayQbr = GetStatValue(awayPassingStats, Passing.QBR),
                AwayCompletionPercent = GetStatValue(awayPassingStats, Passing.CompletionPercent),
                AwayNetPassYardsPerGame = GetStatValue(awayPassingStats, Passing.NetPassYardsPerGame),
                AwayPassingBigPlays = GetStatValue(awayPassingStats, Passing.PassingBigPlays),
                AwayScrimmageYardsPerGame = GetStatValue(awayPassingStats, Passing.ScrimmageYardsPerGame),
                AwayYardsPerPassAttempt = GetStatValue(awayPassingStats, Passing.YardsPerPassAttempt),
                AwayRbRating = GetStatValue(awayRushingStats, Rushing.RbRating),
                AwayRushYardsPerGame = GetStatValue(awayRushingStats, Rushing.RushYardsPerGame),
                AwayYardsPerRushAttempt = GetStatValue(awayRushingStats, Rushing.YardsPerRushAttempt),
                AwayRushingTouchdowns = GetStatValue(awayRushingStats, Rushing.RushingTouchdowns),
                AwayWrRating = GetStatValue(awayReceivingStats, Receiving.WrRating),
                AwayReceivingTouchdowns = GetStatValue(awayReceivingStats, Receiving.ReceivingTouchdowns),
                AwayReceivingYardsPerGame = GetStatValue(awayReceivingStats, Receiving.ReceivingYardsPerGame),
                AwayReceivingYardsPerReception = GetStatValue(awayReceivingStats, Receiving.ReceivingYardsPerReception),
                AwayAverageSackYards = GetStatValue(awayDefenseStats, Defense.AverageSackYards),
                AwayAverageStuffYards = GetStatValue(awayDefenseStats, Defense.AverageStuffYards),
                AwayDefensiveTouchdowns = GetStatValue(awayDefenseStats, Defense.DefensiveTouchdowns),
                AwaySacks = GetStatValue(awayDefenseStats, Defense.Sacks),
                AwayExtraPointPercentage = GetStatValue(awayKickingStats, Kicking.ExtraPointPercentage),
                AwayFieldGoalPercent = GetStatValue(awayKickingStats, Kicking.FieldGoalPercent),
                AwayFieldGoalsMade = GetStatValue(awayKickingStats, Kicking.FieldGoalsMade),
                AwayTotalKickingPoints = GetStatValue(awayKickingStats, Kicking.TotalKickingPoints),
                AwayNetAveragePuntYards = GetStatValue(awayPuntingStats, Punting.NetAveragePuntYards),
                AwayTotalPunts = GetStatValue(awayPuntingStats, Punting.TotalPunts),
                AwayPuntsInsideTwenty = GetStatValue(awayPuntingStats, Punting.PuntsInsideTwenty),
                AwayDefensivePoints = GetStatValue(awayScoringStats, Scoring.DefensivePoints),
                AwayPassingTouchdowns = GetStatValue(awayScoringStats, Scoring.PassingTouchdowns),
                AwayTotalPoints = GetStatValue(awayScoringStats, Scoring.TotalPoints),
                AwayFirstDownsPerGame = GetStatValue(awayMiscStats, Miscellaneous.FirstDownsPerGame),
                AwayFourthDownConversionPercent = GetStatValue(awayMiscStats, Miscellaneous.FourthDownConversionPercent),
                AwayThirdDownConversionPercent = GetStatValue(awayMiscStats, Miscellaneous.ThirdDownConversionPercent),
                AwayRedZoneEfficiencyPercent = GetStatValue(awayMiscStats, Miscellaneous.RedZoneEfficiencyPercent),
                AwayTurnoverDifferential = GetStatValue(awayMiscStats, Miscellaneous.TurnoverDifferential),
            };
        }

        private static decimal GetStatValue(IEnumerable<CategoryStat> stats, string name)
        {
            if (stats == null)
                return 0;

            var stat = stats.FirstOrDefault(s => s.Name == name);
            if (stat == null)
                return 0;

            return stat.Value;
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