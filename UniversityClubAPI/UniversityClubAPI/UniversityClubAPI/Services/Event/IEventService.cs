using UniversityClubAPI.DTOs;
using UniversityClubAPI.DTOs.Club;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Event;
using UniversityClubAPI.Helpers;
namespace UniversityClubAPI.Services.EventService
{
    public interface IEventService
    {
        Task<ApiResponse<EventResponseDto>> CreateAsync(int userId, CreateEventDto dto);
        Task<ApiResponse<EventResponseDto>> UpdateAsync(int userId, int eventId, CreateEventDto dto);
        Task<ApiResponse<string>> DeleteAsync(int userId, int eventId);
        Task<ApiResponse<string>> JoinAsync(int userId, int eventId);
        Task<ApiResponse<string>> LeaveAsync(int userId, int eventId);
        Task<ApiResponse<EventJoinStatusDto>> GetJoinStatusAsync(int userId, int eventId);
        Task<ApiResponse<List<EventJoinRequestDto>>> GetJoinRequestsAsync(int userId, int eventId);
        Task<ApiResponse<string>> RespondToJoinRequestAsync(int moderatorId, int eventId, int requestId, bool approve);
        Task<ApiResponse<PagedResultDto<EventSummaryDto>>> GetAllAsync(int page, int pageSize);
        Task<ApiResponse<EventResponseDto>> GetByIdAsync(int eventId);
        Task<ApiResponse<PagedResultDto<EventSummaryDto>>> GetUpcomingAsync(int page, int pageSize);
        Task<ApiResponse<PagedResultDto<EventSummaryDto>>> SearchAsync(string keyword, int? clubId, int page, int pageSize);
        Task<ApiResponse<PagedResultDto<EventSummaryDto>>> GetByClubAsync(int clubId, int page, int pageSize);
        Task<ApiResponse<PagedResultDto<EventSummaryDto>>> GetMyEventsAsync(int userId, int page, int pageSize);
        Task<ApiResponse<PagedResultDto<MyJoinedEventDto>>> GetMyJoinedEventsAsync(int userId, int page, int pageSize);
        Task<ApiResponse<List<ClubUpcomingEventDto>>> GetMyClubsUpcomingAsync(int userId);
        Task<ApiResponse<List<EventAttendeeDto>>> GetAttendeesAsync(int eventId);
        Task<ApiResponse<EventStatsDto>> GetStatsAsync(int userId, int eventId);
    }
}