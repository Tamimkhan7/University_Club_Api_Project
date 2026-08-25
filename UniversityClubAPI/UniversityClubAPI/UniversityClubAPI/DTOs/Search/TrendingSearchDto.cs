namespace UniversityClubAPI.DTOs.Search
{
    public class TrendingSearchDto
    {
        public string Query { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}