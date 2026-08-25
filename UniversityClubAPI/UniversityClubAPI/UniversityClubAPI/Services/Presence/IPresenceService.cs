using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Presence;
using UniversityClubAPI.Helpers;
namespace UniversityClubAPI.Services.PresenceService
{
    public interface IPresenceService
    {
        Task<ApiResponse<PresenceStatusDto>> GetStatusAsync(int userId);
        Task<ApiResponse<List<PresenceStatusDto>>> GetBulkStatusAsync(List<int> userIds);
        Task<ApiResponse<PagedResultDto<PresenceStatusDto>>> GetOnlineFollowingAsync(int currentUserId, PaginationParamsDto pagination);
    }
}