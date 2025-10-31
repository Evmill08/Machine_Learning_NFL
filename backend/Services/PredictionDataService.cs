using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using backend.enums;
using backend.Models;

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

        public PredictionDataService(ISeasonService seasonService)
        {
            _seasonService = seasonService;
        }

        public async Task<IEnumerable<PredictionData>> GetPredictionDataForTimeframe(int startYear, int endYear)
        {
            var seasons = await _seasonService.GetSeasonsRangedAsync(startYear, endYear);

            var predictionData = new ConcurrentBag<PredictionData>();
            using var semaphore = new SemaphoreSlim(4); // global cap for total work

            var tasks = seasons
                .Where(season => season.SeasonType?.Weeks != null) // only process seasons with weeks
                .SelectMany(season =>
                    season.SeasonType!.Weeks!.Select(async week =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            foreach (var e in week.Events ?? Enumerable.Empty<Event>())
                            {
                                var data = GetPredictionDataForEvent(e);
                                predictionData.Add(data);
                            }
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    })
                );

            await Task.WhenAll(tasks);
            return predictionData.OrderBy(pd => pd.SeasonYear);
        }

        public async Task<IEnumerable<PredictionData>> GetPredictionDataForYear(int year)
        {
            var season = await _seasonService.GetSeasonByYearAsync(year);
            var predictionData = new ConcurrentBag<PredictionData>();

            Parallel.ForEach(
                season.SeasonType.Weeks,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                week =>
                {
                    foreach (var e in week.Events ?? Enumerable.Empty<Event>())
                    {
                        var data = GetPredictionDataForEvent(e);
                        predictionData.Add(data);
                    }
                });

            return predictionData.OrderBy(pd => pd.WeekNumber);
        }


        public PredictionData GetPredictionDataForEvent(Event game)
        {
            var competition = game.Competitions.FirstOrDefault();
            var homeTeam = competition.Competitors.Where(team => team.HomeAway == "home").FirstOrDefault();
            var awayTeam = competition.Competitors.Where(team => team.HomeAway == "away").FirstOrDefault();

            var homeOddsRecord = homeTeam.Team.OddsRecord?.OddsStats ?? Enumerable.Empty<OddsStat>();
            var awayOddsRecord = awayTeam.Team.OddsRecord?.OddsStats ?? Enumerable.Empty<OddsStat>();

            var homePassingStats = homeTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Passing).FirstOrDefault().CategoryStats;
            var homeRushingStats = homeTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Rushing).FirstOrDefault().CategoryStats;
            var homeReceivingStats = homeTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Receiving).FirstOrDefault().CategoryStats;
            var homeKickingStats = homeTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Kicking).FirstOrDefault().CategoryStats;
            var homePuntingStats = homeTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Punting).FirstOrDefault().CategoryStats;
            var homeDefenseStats = homeTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Defense).FirstOrDefault().CategoryStats;
            var homeScoringStats = homeTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Scoring).FirstOrDefault().CategoryStats;
            var homeMiscStats = homeTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Misc).FirstOrDefault().CategoryStats;

            var awayPassingStats = awayTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Passing).FirstOrDefault().CategoryStats;
            var awayRushingStats = awayTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Rushing).FirstOrDefault().CategoryStats;
            var awayReceivingStats = awayTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Receiving).FirstOrDefault().CategoryStats;
            var awayKickingStats = awayTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Kicking).FirstOrDefault().CategoryStats;
            var awayPuntingStats = awayTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Punting).FirstOrDefault().CategoryStats;
            var awayDefenseStats = awayTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Defense).FirstOrDefault().CategoryStats;
            var awayScoringStats = awayTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Scoring).FirstOrDefault().CategoryStats;
            var awayMiscStats = awayTeam.Team.Statistics.StatCategories.Where(sc => sc.Name == StatCategories.Misc).FirstOrDefault().CategoryStats;

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
                NuetralSite = competition.NuetralSite ? 1 : 0,
                DivisionCompetition = competition.DivisionCompetition ? 1 : 0,
                ConferenceCompetition = competition.ConferenceCompetition ? 1 : 0,
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
                HomeTeamMlWins = homeOddsRecord.FirstOrDefault(o => o.OddsRecord == OddsRecords.Moneyline)?.Wins ?? 0,
                HomeTeamMlLosses = homeOddsRecord.FirstOrDefault(o => o.OddsRecord == OddsRecords.Moneyline)?.Losses ?? 0,
                HomeTeamSpreadWins = homeOddsRecord.FirstOrDefault(o => o.OddsRecord == OddsRecords.SpreadOverall)?.Wins ?? 0,
                HomeTeamSpreadLosses = homeOddsRecord.FirstOrDefault(o => o.OddsRecord == OddsRecords.SpreadOverall)?.Losses ?? 0,
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
                AwayTeamMlWins = awayOddsRecord.FirstOrDefault(o => o.OddsRecord == OddsRecords.Moneyline)?.Wins ?? 0,
                AwayTeamMlLosses = awayOddsRecord.FirstOrDefault(o => o.OddsRecord == OddsRecords.Moneyline)?.Losses ?? 0,
                AwayTeamSpreadWins = awayOddsRecord.FirstOrDefault(o => o.OddsRecord == OddsRecords.SpreadOverall)?.Wins ?? 0,
                AwayTeamSpreadLosses = awayOddsRecord.FirstOrDefault(o => o.OddsRecord == OddsRecords.SpreadOverall)?.Losses ?? 0,
                AwayInjuryCount = awayTeam.Team.Injuries.InjuryCount,
                AveragePredictedTotal = competition.CompetitionOdds.AverageOverUnder,
                AveragePredictedSpread = competition.CompetitionOdds.AverageSpread,
                MoneyLineWinner = competition.CompetitionOdds.MoneyLineWinner ? 1 : 0,
                SpreadWinner = competition.CompetitionOdds.SpreadWinner ? 1 : 0,
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
                HomeYardsAllowed = GetStatValue(homeDefenseStats, Defense.YardsAllowed),
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
                AwayYardsAllowed = GetStatValue(awayDefenseStats, Defense.YardsAllowed),
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
        

        private static decimal GetPredictorValue(IEnumerable<Statistic> compStats, string name){
            if (compStats == null)
                return 0;

            var compStat = compStats.FirstOrDefault(s => s.Name == name);
            if (compStat == null)
                return 0;

            return compStat.Value;
        }
        

    }
}