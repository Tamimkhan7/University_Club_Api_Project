using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Search
{
    public class AdvancedSearchDto
    {
        public SearchEntityType Type { get; set; } = SearchEntityType.Posts;
        public string? Query { get; set; }
        public int? ClubId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public SearchSortBy SortBy { get; set; } = SearchSortBy.Relevance;
    }

    public class AdvancedSearchResultDto
    {
        public SearchEntityType Type { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<UserSearchItemDto>? Users { get; set; }
        public List<ClubSearchItemDto>? Clubs { get; set; }
        public List<PostSearchItemDto>? Posts { get; set; }
        public List<EventSearchItemDto>? Events { get; set; }
        public List<GroupSearchItemDto>? Groups { get; set; }
        public List<FileSearchItemDto>? Files { get; set; }
    }
}