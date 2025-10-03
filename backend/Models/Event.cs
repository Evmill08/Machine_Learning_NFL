using backend.DTOs;

namespace backend.Models
{
    public class Event
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public bool TimeValid { get; set; }
        public List<Competition> Competitions { get; set; }
        public int Season { get; set; }
        public int Week { get; set; }
    }
}