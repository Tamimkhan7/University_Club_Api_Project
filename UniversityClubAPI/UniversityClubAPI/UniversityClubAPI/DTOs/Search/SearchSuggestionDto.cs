using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Search
{
    public class SearchSuggestionDto
    {
        public SearchEntityType Type { get; set; }
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}