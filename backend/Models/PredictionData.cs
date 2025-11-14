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
        public string HomeTeamId { get; set; }
        public string AwayTeamId { get; set; }
        public int HomeWinner { get; set; } // 1 if true
        public int AwayWinner { get; set; } // 1 if true
        public decimal AveragePredictedTotal { get; set; }
        public decimal AveragePredictedSpread { get; set; }
        public VegasPrediction BestPredictedTotal { get; set; }
        public VegasPrediction BestPredictedSpread { get; set; }
        public decimal PredictedHomeWinPercent { get; set; }
        public decimal PredicitedHomeMatchupQuality { get; set; }
        public decimal PredictedHomeLossPercent { get; set; }
        public decimal PredictedHomeDefenseEfficiency { get; set; }
        public decimal PredictedHomeOffenseEfficiency { get; set; }
        public decimal PredictedHomeTotalEfficiency { get; set; }
        public decimal PredictedHomePointDifferential { get; set; }
        public decimal PredictedAwayWinPercent { get; set; }
        public decimal PredicitedAwayMatchupQuality { get; set; }
        public decimal PredictedAwayLossPercent { get; set; }
        public decimal PredictedAwayPointDifferential { get; set; }
        public decimal PredictedAwayDefenseEfficiency { get; set; }
        public decimal PredictedAwayOffenseEfficiency { get; set; }
        public decimal PredictedAwayTotalEfficiency { get; set; }
    }
    
    public class VegasPrediction
    {
        public string SportsBook { get; set; }
        public decimal OddsValue { get; set; }
    }
}