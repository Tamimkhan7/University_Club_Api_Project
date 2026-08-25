using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.LiveEvent;
using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.LiveEventService
{
    public interface ILiveEventService
    {
        Task<ApiResponse<LiveSessionResponseDto>> StartLiveAsync(int userId, int eventId, StartLiveDto dto);
        Task<ApiResponse<LiveSessionResponseDto>> EndLiveAsync(int userId, int eventId);
        Task<ApiResponse<LiveSessionResponseDto>> GetStatusAsync(int userId, int eventId);
        Task<ApiResponse<PagedResultDto<LiveChatMessageDto>>> GetChatHistoryAsync(int userId, int eventId, PaginationParamsDto pagination);
        Task<ApiResponse<List<LiveViewerDto>>> GetActiveViewersAsync(int userId, int eventId);

        Task<ApiResponse<LiveModerationStatusDto>> MuteUserAsync(int moderatorId, int eventId, int targetUserId, MuteRequestDto dto);
        Task<ApiResponse<LiveModerationStatusDto>> KickUserAsync(int moderatorId, int eventId, int targetUserId, KickRequestDto dto);
        Task<ApiResponse<string>> UnbanUserAsync(int moderatorId, int eventId, int targetUserId);
    }
}