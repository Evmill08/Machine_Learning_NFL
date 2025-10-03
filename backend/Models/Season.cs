using backend.DTOs;

namespace backend.Models
{
    public class Season
    {
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SeasonType SeasonType { get; set; }
    }

    public class SeasonType
    {
        public string Id { get; set; }
        public int Type { get; set; }
        public Week Week { get; set; }
        public List<Week> Weeks { get; set; }
    }
}