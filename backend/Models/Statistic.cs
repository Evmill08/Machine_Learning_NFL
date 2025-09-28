// This is a general stat model that is used pretty often to represent differet
// stat objects ESPN gives us 

namespace backend.Models
{
    public class Statistic
    {
        public string Name;
        public string DisplayName;
        public string Description;
        public decimal Value;
        public string DisplayValue;
    }
}
