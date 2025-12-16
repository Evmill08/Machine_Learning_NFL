using backend.DTOs;

namespace backend.Models
{
    public class Competition
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public bool TimeValid { get; set; }
        public bool DateValid { get; set; }
        public Venue Venue {get; set;}
        public Weather Weather {get; set;}
        public List<Competitor> Competitors { get; set; }
        public List<Odds> CompetitionOdds { get; set; }
        public Predictors CompetitionPredictors { get; set; }
    }

    public class Weather
    {
        public double MeanTemperature {get; set;}
        public double MaxTemperature {get; set;}
        public double MinTemperature {get; set;}
        public double ApparentTemperature {get; set;}
        public double PrecipitationSum {get; set;}
        public double SnowfallSum {get; set;}
        public double PrecipitationHours {get; set;}
        public double RainSum {get; set;}
        public double WindSpeedMax {get; set;}
        public double WindGustsMax {get; set;}
        public double DominantWindDirection {get; set;}
        public double MeanRelativeHumidity {get; set;}
        public double MeanWindGusts {get; set;}
        public double MeanWindSpeed {get; set;}
    }

    public class Venue
    {
        public string StadiumName {get; set;}
        public string City {get; set;}
        public string State {get; set;}
        public string ZipCode {get; set;}
        public string Country {get; set;}
        public bool Grass {get; set;}
        public bool Indoors {get; set;}
        public bool OutOfUSA {get; set;}
    }

    public class Competitor
    {
        public string Id { get; set; }
        public string HomeAway { get; set; }
        public bool Winner { get; set; }
        public Team Team { get; set; }
        public Score CompetitorScore { get; set; }
    }

    public class Odds
    {
        public string Details { get; set; }
        public decimal OverUnder { get; set; }
        public decimal Spread { get; set; }
        public string Provider { get; set; }
    }

    public class Predictors
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public DateTime LastModified { get; set; }
        public TeamPredictor HomeTeamPredictors { get; set; }
        public TeamPredictor AwayTeamPredictors { get; set; }
    }

    public class TeamPredictor
    {
        public List<Statistic> Predictors { get; set; }
    }
    
    public class Score
    {
        public decimal Value { get; set; }
        public string DisplayValue { get; set; }
        public bool Winner { get; set; }
    }

}
