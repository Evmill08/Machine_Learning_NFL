using System.Text.Json.Serialization;

namespace backend.Models
{
    public class GamePrediction
    {
        [JsonPropertyName("spread_prediction")]
        public double SpreadPrediction { get; set; }
        
        [JsonPropertyName("spread_confidence_range")]
        public List<double> SpreadRange { get; set; }
        
        [JsonPropertyName("spread_confidence_score")]
        public double SpreadConfidenceScore { get; set; }
        
        [JsonPropertyName("total_prediction")]
        public double TotalPrediction { get; set; }
        
        [JsonPropertyName("total_confidence_range")]
        public List<double> TotalRange { get; set; }
        
        [JsonPropertyName("total_confidence_score")]
        public double TotalConfidenceScore { get; set; }
        
        [JsonPropertyName("winner_prediction")]
        public string WinnerPrediction { get; set; }
        
        [JsonPropertyName("winner_confidence")]
        public double WinnerConfidence { get; set; }

        [JsonPropertyName("home_win_probability")]
        public double HomeWinProbability { get; set; }

        [JsonPropertyName("away_win_probability")]
        public double AwayWinProbability { get; set; }
    }
}