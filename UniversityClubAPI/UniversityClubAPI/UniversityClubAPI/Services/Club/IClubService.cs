using UniversityClubAPI.DTOs.Club;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.Helpers;
namespace UniversityClubAPI.Services.ClubService
{
    public interface IClubService
    {
        Task<ApiResponse<object>> CreateClubAsync(int userId, CreateClubDTO dto);
        Task<ApiResponse<object>> JoinClubAsync(int userId, JoinClubDTO dto);
        Task<ApiResponse<string>> LeaveClubAsync(int userId, int clubId);
        Task<ApiResponse<object>> GetAllClubsAsync(PaginationParamsDto pagination);
        Task<ApiResponse<object>> GetClubByIdAsync(int userId, int clubId);
        Task<ApiResponse<object>> UpdateClubAsync(int userId, int clubId, CreateClubDTO dto);
        Task<ApiResponse<string>> DeleteClubAsync(int userId, int clubId);
        Task<ApiResponse<object>> GetMembersAsync(int userId, int clubId, PaginationParamsDto pagination);
        Task<ApiResponse<string>> UpdateRoleAsync(int callerId, int clubId, UpdateClubRoleDto dto);
        Task<ApiResponse<string>> RemoveMemberAsync(int callerId, int clubId, int targetUserId);
        Task<ApiResponse<object>> SearchClubsAsync(string query, PaginationParamsDto pagination);
        Task<ApiResponse<object>> GetMyClubsAsync(int userId);
        Task<ApiResponse<object>> GetMembershipStatusAsync(int userId, int clubId);
        Task<ApiResponse<object>> GetClubPostsAsync(int userId, int clubId, PaginationParamsDto pagination);
        Task<ApiResponse<object>> SearchMembersAsync(int userId, int clubId, string query, PaginationParamsDto pagination);
    }
}