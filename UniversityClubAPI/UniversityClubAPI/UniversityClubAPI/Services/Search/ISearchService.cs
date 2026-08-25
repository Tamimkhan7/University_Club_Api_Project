using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Search;
using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.SearchService
{
    public interface ISearchService
    {
        Task<ApiResponse<UnifiedSearchResultDto>> GlobalSearchAsync(int userId, string query, int limitPerType = 5);
        Task<ApiResponse<AdvancedSearchResultDto>> AdvancedSearchAsync(int userId, AdvancedSearchDto dto, PaginationParamsDto pagination);
        Task<ApiResponse<List<RecentSearchDto>>> GetRecentSearchesAsync(int userId, int count = 10);
        Task<ApiResponse<string>> DeleteRecentSearchAsync(int userId, int historyId);
        Task<ApiResponse<string>> ClearRecentSearchesAsync(int userId);

        Task<ApiResponse<List<SearchSuggestionDto>>> GetSuggestionsAsync(int userId, string query, int count = 8);
        Task<ApiResponse<List<TrendingSearchDto>>> GetTrendingSearchesAsync(int days = 7, int count = 10);
    }
}