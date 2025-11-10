using System.Net.NetworkInformation;

namespace backend.Models
{
    public class Competition
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public bool TimeValid { get; set; }
        public bool DateValid { get; set; }
        public bool NuetralSite { get; set; }
        public bool DivisionCompetition { get; set; }
        public bool ConferenceCompetition { get; set; }
        public List<Competitor> Competitors { get; set; }
        public Odds CompetitionOdds { get; set; }
        public Predictors CompetitionPredictors { get; set; }
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
        public decimal AverageOverUnder { get; set; }
        public decimal AverageSpread { get; set; }
        public bool MoneyLineWinner { get; set; }
        public bool SpreadWinner { get; set; }
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
