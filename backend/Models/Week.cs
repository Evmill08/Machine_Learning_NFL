namespace backend.Models
{
    public class Week
    {
        public int WeekNumber;
        public DateTime StartDate;
        public DateTime EndDate;
        public List<Event> Events;
    }
}