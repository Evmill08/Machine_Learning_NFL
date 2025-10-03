namespace backend.Models
{
    public class Team
    {
        public string Id { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Record Record { get; set; }
        public OddsRecord OddsRecord { get; set; }
        public Statistics Statistics { get; set; }
        public Injuries Injuries { get; set; }
    }

    public class Record
    {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public decimal AveragePointsAgainst { get; set; }
        public decimal AveragePointsFor { get; set; }
        public decimal PointDifferential { get; set; }
        public decimal DivisionWinPercent { get; set; }
        public decimal LeagueWinPercent { get; set; }
        public int Streak { get; set; }
        public int Ties { get; set; }
        public decimal WinPercent { get; set; }
        public int DivisionLosses { get; set; }
        public int DivisionWins { get; set; }
        public int HomeWins { get; set; }
        public int HomeLosses { get; set; }
        public int AwayWins { get; set; }
        public int AwayLosses { get; set; }
        public int ConferenceWins { get; set; }
        public int ConferenceLosses { get; set; }
    }

    public class OddsRecord
    {
        public List<OddsStat> OddsStats { get; set; }
    }

    public class OddsStat
    {
        public string OddsRecord { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
    }

    public class Injuries
    {
        public int InjuryCount { get; set; }
    }
}