using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Recruitment;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
namespace UniversityClubAPI.Services.RecruitmentService
{
    public interface IRecruitmentService
    {
        Task<ApiResponse<ApplicationResponseDto>> ApplyAsync(int userId, int clubId, CreateApplicationDto dto);
        Task<ApiResponse<string>> WithdrawApplicationAsync(int userId, int applicationId);
        Task<ApiResponse<PagedResultDto<ApplicationResponseDto>>> GetMyApplicationsAsync(int userId, PaginationParamsDto pagination);
        Task<ApiResponse<PagedResultDto<ApplicationResponseDto>>> GetClubApplicationsAsync(int currentUserId, int clubId, ApplicationStatus? status, PaginationParamsDto pagination);
        Task<ApiResponse<ApplicationResponseDto>> ApproveApplicationAsync(int currentUserId, int applicationId, ReviewApplicationDto dto);
        Task<ApiResponse<ApplicationResponseDto>> RejectApplicationAsync(int currentUserId, int applicationId, ReviewApplicationDto dto);
        Task<ApiResponse<int>> GetPendingCountAsync(int currentUserId, int clubId);
    }
}