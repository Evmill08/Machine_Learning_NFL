namespace backend.Models
{
    public class PredictionResponse
    {
        public string HomeTeamName {get; set;}
        public string AwayTeamName {get; set;}
        public string EventId { get; set; }
        public DateTime Date { get; set; }
        public GamePrediction GamePrediction { get; set; }
        public decimal VegasLowestSpread { get; set; }
        public decimal VegasLowestTotal { get; set; }
        public string VegasWinner { get; set; } 
    }   
}
