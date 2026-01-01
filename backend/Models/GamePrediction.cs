using System.Text.Json.Serialization;

namespace backend.Models
{
    public class GamePredictionResponse
    {
        public double SpreadPrediction { get; set; }
        public List<double> SpreadRange { get; set; }
        public double TotalPrediction { get; set; }
        public List<double> TotalRange { get; set; }
        public string WinnerPrediction { get; set; }
        public double HomeWinProbability { get; set; }
        public double AwayWinProbability { get; set; }
    }
}