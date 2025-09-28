using backend.DTOs;

namespace backend.Models
{
    public class Event
    {
        public string Id;
        public DateTime Date;
        public string Name;
        public bool TimeValid;
        public List<Competition> Competitions;
    }
}