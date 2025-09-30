using backend.DTOs;

namespace backend.Models
{
    public class Season
    {
        public int Year;
        public DateTime StartDate;
        public DateTime EndDate;
        public SeasonType SeasonType;
    }

    public class SeasonType
    {
        public string Id;
        public int Type;
        public Week Week;
        public List<Week> Weeks;
    }
}