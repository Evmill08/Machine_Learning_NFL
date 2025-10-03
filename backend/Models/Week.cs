namespace backend.Models
{
    public class Week
    {
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Event> Events { get; set; }
    }
}