using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.User;
using UniversityClubAPI.Helpers;
namespace UniversityClubAPI.Services.UserService
{
    public interface IUserService
    {
        Task<ApiResponse<UserProfileDto>> GetMyProfileAsync(int callerId);
        Task<ApiResponse<UserPublicDto>> GetProfileByIdAsync(int callerId, int targetUserId);
        Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int callerId, UpdateUserDto dto);
        Task<ApiResponse<string>> ChangePasswordAsync(int callerId, ChangePasswordDto dto);
        Task<ApiResponse<string>> DeactivateAsync(int callerId);
        Task<ApiResponse<string>> SoftDeleteAsync(int callerId);
        Task<ApiResponse<string>> SetPrivacyAsync(int callerId, bool isPrivate);
        Task<ApiResponse<PagedResultDto<UserSummaryDto>>> GetAllAsync(int callerId, UserQueryDto query);
        Task<ApiResponse<PagedResultDto<UserSummaryDto>>> SearchAsync(int callerId, UserQueryDto query);
        Task<ApiResponse<string>> FollowAsync(int callerId, int targetUserId);
        Task<ApiResponse<string>> UnfollowAsync(int callerId, int targetUserId);
        Task<ApiResponse<PagedResultDto<UserSummaryDto>>> GetFollowersAsync(int callerId, int targetUserId, PaginationParamsDto pagination);
        Task<ApiResponse<PagedResultDto<UserSummaryDto>>> GetFollowingAsync(int callerId, int targetUserId, PaginationParamsDto pagination);
        Task<ApiResponse<PagedResultDto<UserSummaryDto>>> GetMutualFollowsAsync(int callerId, int targetUserId, PaginationParamsDto pagination);
        Task<ApiResponse<string>> BlockAsync(int callerId, int targetUserId);
        Task<ApiResponse<string>> UnblockAsync(int callerId, int targetUserId);
        Task<ApiResponse<PagedResultDto<UserSummaryDto>>> GetBlockedAsync(int callerId, PaginationParamsDto pagination);
        Task<ApiResponse<UserStatsDto>> GetStatsAsync(int targetUserId);
        Task<ApiResponse<string>> RecordProfileViewAsync(int callerId, int targetUserId);
    }
}