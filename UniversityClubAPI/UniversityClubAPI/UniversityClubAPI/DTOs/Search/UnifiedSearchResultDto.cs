namespace UniversityClubAPI.DTOs.Search
{
    public class UnifiedSearchResultDto
    {
        public string Query { get; set; } = string.Empty;
        public List<UserSearchItemDto> Users { get; set; } = new();
        public List<ClubSearchItemDto> Clubs { get; set; } = new();
        public List<PostSearchItemDto> Posts { get; set; } = new();
        public List<EventSearchItemDto> Events { get; set; } = new();
        public List<GroupSearchItemDto> Groups { get; set; } = new();
        public List<FileSearchItemDto> Files { get; set; } = new();
        public int TotalResults { get; set; }
    }
}