namespace backend.Models
{
    public class Competition
    {
        public string Id;
        public DateTime Date;
        public bool TimeValid;
        public bool DateValid;
        public bool NuetralSite;
        public bool DivisionCompetition;
        public bool ConferenceCompetition;
        public List<Competitor> Competitors;
        public Odds CompetitionOdds;
        public Predictors CompetitionPredictors;
    }

    public class Competitor
    {
        public string Id;
        public string HomeAway;
        public bool Winner;
        public Team Team;
        public Score CompetitorScore;
    }

    public class Odds
    {
        public string Details;
        public decimal AverageOverUnder;
        public decimal AverageSpread;
        public bool MoneyLineWinner;
        public bool SpreadWinner;
    }

    public class Predictors
    {
        public string Name;
        public string ShortName;
        public DateTime LastModified;
        public TeamPredictor HomeTeamPredictors;
        public TeamPredictor AwayTeamPredictors;
    }

    public class TeamPredictor
    {
        public List<Statistic> Predictors;
    }
    
    public class Score
    {
        public decimal Value;
        public string DisplayValue;
        public bool Winner;
    }

}
