// This is a general stat model that is used pretty often to represent differet
// stat objects ESPN gives us 

namespace backend.Models
{
    public class Statistic
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
        public string DisplayValue { get; set; }
    }
}
