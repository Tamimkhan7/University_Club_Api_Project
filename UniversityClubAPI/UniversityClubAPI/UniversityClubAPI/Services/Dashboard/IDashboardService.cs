using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<ApiResponse<object>> GetStatsAsync(int userId);
        Task<ApiResponse<object>> GetTrendingPostsAsync();
        Task<ApiResponse<object>> GetSummaryAsync(int userId);
        Task<ApiResponse<object>> GetRecentPostsAsync(int userId);
        Task<ApiResponse<object>> GetRecentClubsAsync(int userId);
        Task<ApiResponse<object>> GetAiInsightAsync(int userId);
    }
}