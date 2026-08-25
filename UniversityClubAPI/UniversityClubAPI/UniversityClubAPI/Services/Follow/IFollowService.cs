using UniversityClubAPI.DTOs;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Follow;
using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.FollowService
{
    public interface IFollowService
    {

        Task<ApiResponse<string>> FollowAsync(int currentUserId, int targetUserId);
        Task<ApiResponse<string>> UnfollowAsync(int currentUserId, int targetUserId);

        Task<ApiResponse<PagedResultDto<FollowUserDto>>> GetMyFollowersAsync(int currentUserId, PaginationParamsDto pagination);
        Task<ApiResponse<PagedResultDto<FollowUserDto>>> GetMyFollowingAsync(int currentUserId, PaginationParamsDto pagination);
        Task<ApiResponse<PagedResultDto<FollowUserDto>>> GetUserFollowersAsync(int currentUserId, int targetUserId, PaginationParamsDto pagination);
        Task<ApiResponse<PagedResultDto<FollowUserDto>>> GetUserFollowingAsync(int currentUserId, int targetUserId, PaginationParamsDto pagination);

        Task<ApiResponse<FollowStatusDto>> GetFollowStatusAsync(int currentUserId, int targetUserId);
        Task<ApiResponse<FollowCountsDto>> GetFollowCountsAsync(int currentUserId, int targetUserId);
        Task<ApiResponse<List<SuggestedUserDto>>> GetSuggestionsAsync(int currentUserId);
        Task<ApiResponse<List<SuggestedUserDto>>> GetCommonSuggestionsAsync(int currentUserId);
        Task<ApiResponse<PagedResultDto<SuggestedUserDto>>> SearchUsersAsync(int currentUserId, string query, PaginationParamsDto pagination);
        Task<ApiResponse<List<MutualUserDto>>> GetMutualFollowingAsync(int currentUserId, int targetUserId);
        Task<ApiResponse<string>> BlockUserAsync(int currentUserId, int targetUserId);
        Task<ApiResponse<string>> UnblockUserAsync(int currentUserId, int targetUserId);
        Task<ApiResponse<PagedResultDto<BlockedUserDto>>> GetBlockedUsersAsync(int currentUserId, PaginationParamsDto pagination);
        Task<ApiResponse<BlockStatusDto>> GetBlockStatusAsync(int currentUserId, int targetUserId);
    }
}
