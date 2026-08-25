using UniversityClubAPI.DTOs.Leaderboard;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
namespace UniversityClubAPI.Services.LeaderboardService
{
    public interface ILeaderboardService
    {
        Task<ApiResponse<LeaderboardResultDto>> GetLeaderboardAsync(int currentUserId, LeaderboardCategory category, LeaderboardPeriod period, int count = 20);
        Task<ApiResponse<LeaderboardEntryDto?>> GetMyLeaderboardEntryAsync(int currentUserId, LeaderboardCategory category, LeaderboardPeriod period);
        Task<ApiResponse<LeaderboardEntryDto?>> GetUserLeaderboardEntryAsync(int currentUserId, int targetUserId, LeaderboardCategory category, LeaderboardPeriod period);
        Task<ApiResponse<LeaderboardInsightDto>> GetLeaderboardInsightAsync(int currentUserId, LeaderboardCategory category, LeaderboardPeriod period);
    }
}