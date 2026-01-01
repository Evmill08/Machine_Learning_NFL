namespace backend.Models
{
    public class PredictionResponse
    {
        public string HomeTeamName {get; set;}
        public string AwayTeamName {get; set;} // probably dont need these for front end
        public string EventId { get; set; }
        public DateTime Date { get; set; }
        public GamePredictionResponse GamePrediction { get; set; }
        public VegasPrediction VegasLowestSpread { get; set; }
        public VegasPrediction VegasLowestTotal { get; set; }
        public string VegasWinner { get; set; } 
    }   
}
