namespace UniversityClubAPI.DTOs.Search
{
    public class RecentSearchDto
    {
        public int Id { get; set; }
        public string Query { get; set; } = string.Empty;
        public DateTime SearchedAt { get; set; }
    }
}