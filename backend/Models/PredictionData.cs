using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace backend.Models
{
    // TODO: Add our response variables: Total Points, Spread, MoneyLine
    // This is where we need to aggregate all the data for each team
    // Need to look at all the data we have and decide what is necessary and helpful for each model
    public class PredictionData
    {
        public int TotalPoints { get; set; }
        public int Spread { get; set; } // Positive if home team
        public int HomeWin { get; set; } // this is money line, 0 if home
        public int SeasonYear { get; set; }
        public int WeekNumber { get; set; }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public int NuetralSite { get; set; } // 1 if true 
        public int DivisionCompetition { get; set; } // 1 if true
        public int ConferenceCompetition { get; set; } // 1 if true
        public int HomeWinner { get; set; } // 1 if true
        public int HomeWins { get; set; }
        public int HomeLosses { get; set; }
        public decimal HomeAveragePointsAgainst { get; set; }
        public decimal HomeAveragePointsFor { get; set; }
        public decimal HomePointDifferential { get; set; }
        public int HomeStreak { get; set; }
        public decimal HomeWinPercent { get; set; }
        public int HomeDivisionWins { get; set; }
        public int HomeDivisionLosses { get; set; }
        public int HomeTeamHomeWins { get; set; }
        public int HomeTeamHomeLosses { get; set; }
        public int HomeTeamAwayWins { get; set; }
        public int HomeTeamAwayLosses { get; set; }
        public int HomeConferenceWins { get; set; }
        public int HomeConferenceLosses { get; set; }
        public int HomeTeamMlWins { get; set; }//need 
        public int HomeTeamMlLosses { get; set; }//need 
        public int HomeTeamSpreadWins { get; set; }//need 
        public int HomeTeamSpreadLosses { get; set; } //need 
        public int HomeInjuryCount { get; set; }
        public int AwayWinner { get; set; } // 1 if true
        public int AwayWins { get; set; }
        public int AwayLosses { get; set; }
        public decimal AwayAveragePointsAgainst { get; set; }
        public decimal AwayAveragePointsFor { get; set; }
        public decimal AwayPointDifferential { get; set; }
        public int AwayStreak { get; set; }
        public decimal AwayWinPercent { get; set; }
        public int AwayDivisionWins { get; set; }
        public int AwayDivisionLosses { get; set; }
        public int AwayTeamHomeWins { get; set; }
        public int AwayTeamHomeLosses { get; set; }
        public int AwayTeamAwayWins { get; set; }
        public int AwayTeamAwayLosses { get; set; }
        public int AwayConferenceWins { get; set; }
        public int AwayConferenceLosses { get; set; }
        public int AwayTeamMlWins { get; set; }
        public int AwayTeamMlLosses { get; set; }
        public int AwayTeamSpreadWins { get; set; }
        public int AwayTeamSpreadLosses { get; set; }
        public int AwayInjuryCount { get; set; }
        public decimal AveragePredictedTotal { get; set; }
        public decimal AveragePredictedSpread { get; set; }
        public int MoneyLineWinner { get; set; } // 1 if true
        public int SpreadWinner { get; set; } // 1 if true
        public decimal PredictedHomeWinPercent { get; set; }
        public decimal PredicitedHomeMatchupQuality { get; set; }
        public decimal PredictedHomeLossPercent { get; set; }
        public decimal PredictedHomeDefenseEfficiency { get; set; }
        public decimal PredictedHomeOffenseEfficiency { get; set; }
        public decimal PredictedHomeTotalEfficiency { get; set; }
        public decimal PredictedHomePointDifferential { get; set; }
        public decimal PredictedAwayWinPercent { get; set; }
        public decimal PredicitedAwayMatchupQuality { get; set; }
        public decimal PredictedAwayOpponentStrenth { get; set; }
        public decimal PredictedAwayLossPercent { get; set; }
        public decimal PredictedAwayPointDifferential { get; set; }
        public decimal PredictedAwayDefenseEfficiency { get; set; }
        public decimal PredictedAwayOffenseEfficiency { get; set; }
        public decimal PredictedAwayTotalEfficiency { get; set; }
        public decimal HomeQbr { get; set; } //passing/ESPNQBRating
        public decimal HomeCompletionPercent { get; set; } // passing/completionPct
        public decimal HomeNetPassYardsPerGame { get; set; } // passing/netPassingYards
        public decimal HomePassingBigPlays { get; set; } // passing/passingBigPlays
        public decimal HomeScrimmageYardsPerGame { get; set; } // passing/yardsFromScrimmagePerGame
        public decimal HomeYardsPerPassAttempt { get; set; } // passing/yardsPerPassAttempt
        public decimal HomeRbRating { get; set; } //rushing/ESPNRBRating
        public decimal HomeRushYardsPerGame { get; set; } //rushing/rushingYardsPerGame
        public decimal HomeYardsPerRushAttempt { get; set; } //rushing/yardsPerRushAttempt
        public decimal HomeRushingTouchdowns { get; set; } //rushing/rushingTouchdowns
        public decimal HomeWrRating { get; set; } //receiving/ESPNWRRating
        public decimal HomeReceivingTouchdowns { get; set; } //receiving/receivingTouchdowns
        public decimal HomeReceivingYardsPerGame { get; set; } //receiving/receivingYardsPerGame
        public decimal HomeReceivingYardsPerReception { get; set; } //receiving/yardsPerReception
        public decimal HomeAverageSackYards { get; set; } //defensive/avgSackYards
        public decimal HomeAverageStuffYards { get; set; } //defensive/avgStuffYards
        public decimal HomeDefensiveTouchdowns { get; set; } //defensive/defensiveTouchdowns
        public decimal HomeSacks { get; set; } //defensive/sacks
        public decimal HomeYardsAllowed { get; set; }//defensive/yardsAllowed
        public decimal HomeExtraPointPercentage { get; set; } //kicking/extraPointPct
        public decimal HomeFieldGoalPercent { get; set; } //kicking/fieldGoalPct
        public decimal HomeFieldGoalsMade { get; set; } //kicking/fieldGoalsMade
        public decimal HomeTotalKickingPoints { get; set; } //kicking/totalKickingPoints
        public decimal HomeNetAveragePuntYards { get; set; } //punting/avgPuntReturnYards
        public decimal HomeTotalPunts { get; set; } //punting/punts
        public decimal HomePuntsInsideTwenty { get; set; } //punting/puntsInside20
        public decimal HomeDefensivePoints { get; set; } //scoring/defensivePoints
        public decimal HomePassingTouchdowns { get; set; } //scoring/passingTouchdowns
        public decimal HomeTotalPoints { get; set; } //scoring/totalPoints
        public decimal HomeFirstDownsPerGame { get; set; } //misc/firstDownsPerGame
        public decimal HomeFourthDownConversionPercent { get; set; } //misc/fourthDownConvPct
        public decimal HomeThirdDownConversionPercent { get; set; }
        public decimal HomeRedZoneEfficiencyPercent { get; set; } //misc/redzoneEfficiencyPct
        public decimal HomeTurnoverDifferential { get; set; } //misc/turnOverDifferential
        public decimal AwayQbr { get; set; }
        public decimal AwayCompletionPercent { get; set; }
        public decimal AwayNetPassYardsPerGame { get; set; }
        public decimal AwayPassingBigPlays { get; set; }
        public decimal AwayScrimmageYardsPerGame { get; set; }
        public decimal AwayYardsPerPassAttempt { get; set; }
        public decimal AwayRbRating { get; set; }
        public decimal AwayRushYardsPerGame { get; set; }
        public decimal AwayYardsPerRushAttempt { get; set; }
        public decimal AwayRushingTouchdowns { get; set; }
        public decimal AwayWrRating { get; set; }
        public decimal AwayReceivingTouchdowns { get; set; }
        public decimal AwayReceivingYardsPerGame { get; set; }
        public decimal AwayReceivingYardsPerReception { get; set; }
        public decimal AwayAverageSackYards { get; set; }
        public decimal AwayAverageStuffYards { get; set; }
        public decimal AwayDefensiveTouchdowns { get; set; }
        public decimal AwaySacks { get; set; }
        public decimal AwayYardsAllowed { get; set; }
        public decimal AwayExtraPointPercentage { get; set; }
        public decimal AwayFieldGoalPercent { get; set; }
        public decimal AwayFieldGoalsMade { get; set; }
        public decimal AwayTotalKickingPoints { get; set; }
        public decimal AwayNetAveragePuntYards { get; set; }
        public decimal AwayPuntsInsideTwenty { get; set; }
        public decimal AwayTotalPunts { get; set; }
        public decimal AwayDefensivePoints { get; set; }
        public decimal AwayPassingTouchdowns { get; set; }
        public decimal AwayTotalPoints { get; set; }
        public decimal AwayFirstDownsPerGame { get; set; }
        public decimal AwayFourthDownConversionPercent { get; set; }
        public decimal AwayThirdDownConversionPercent { get; set; }
        public decimal AwayRedZoneEfficiencyPercent { get; set; }
        public decimal AwayTurnoverDifferential { get; set; }

    }
}