namespace backend.Models
{
    public class Statistics
    {
        public List<StatCategory> StatCategories;
    }

    public class StatCategory
    {
        public string Name;
        public string DisplayName;
        public List<CategoryStat> CategoryStats;
    }

    public class CategoryStat
    {
        public string Name;
        public string DisplayName;
        public decimal Value;
        public int Rank;
    }
}