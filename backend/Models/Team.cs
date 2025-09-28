namespace backend.Models
{
    public class Team
    {
        public string Id;
        public string Location;
        public string Name;
        public string DisplayName;
        public Record Record;
        public OddsRecord OddsRecord;
        public Statistics Statistics;
        public Injuries Injuries;
    }

    public class Record
    {
        public int Wins;
        public int Losses;
        public decimal AveragePointsAgainst;
        public decimal AveragePointsFor;
        public decimal PointDifferential;
        public decimal DivisionWinPercent;
        public decimal LeagueWinPercent;
        public int Streak;
        public int Ties;
        public decimal WinPercent;
        public int DivisionLosses;
        public int DivisionWins;
        public int HomeWins;
        public int HomeLosses;
        public int AwayWins;
        public int AwayLosses;
        public int ConferenceWins;
        public int ConferenceLosses;
    }

    public class OddsRecord
    {
        public List<OddsStat> OddsStats;
    }

    public class OddsStat
    {
        public string OddsRecord;
        public int Wins;
        public int Losses;
    }

    public class Injuries
    {
        public int InjuryCount;
    }
}