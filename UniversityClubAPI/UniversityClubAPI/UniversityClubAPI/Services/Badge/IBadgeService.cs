using UniversityClubAPI.DTOs.Badge;
using UniversityClubAPI.Helpers;
namespace UniversityClubAPI.Services.BadgeService
{
    public interface IBadgeService
    {

        Task SeedDefaultBadgesAsync();
        Task<ApiResponse<List<BadgeDto>>> GetCatalogAsync(int userId);
        Task<ApiResponse<List<UserBadgeDto>>> GetMyBadgesAsync(int userId);
        Task<ApiResponse<List<UserBadgeDto>>> GetUserBadgesAsync(int targetUserId);
        Task<ApiResponse<List<UserBadgeDto>>> EvaluateAsync(int userId);
        Task<ApiResponse<List<ContributorLeaderboardDto>>> GetClubLeaderboardAsync(int clubId, int count = 10);

        Task<ApiResponse<string>> RecalculateTopContributorAsync(int currentUserId, int clubId);
        Task<ApiResponse<List<BadgeProgressDto>>> GetProgressAsync(int userId);
        Task<ApiResponse<PagedResultDto<GlobalBadgeLeaderboardDto>>> GetGlobalLeaderboardAsync(int page = 1, int pageSize = 10);
        Task<ApiResponse<string>> RevokeBadgeAsync(int currentUserId, int targetUserId, string badgeCode, int? clubId = null);
        Task<ApiResponse<BadgeHoldersResponseDto>> GetBadgeHoldersAsync(string badgeCode, int? clubId = null, int page = 1, int pageSize = 20);
    }
}
