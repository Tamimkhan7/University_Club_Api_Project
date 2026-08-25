using UniversityClubAPI.DTOs.ClubPrivacy;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
namespace UniversityClubAPI.Services.ClubPrivacyService
{
    public interface IClubPrivacyService
    {

        Task<ApiResponse<string>> UpdateVisibilityAsync(int currentUserId, int clubId, UpdateVisibilityDto dto);
        Task<ApiResponse<InviteResponseDto>> CreateInviteAsync(int currentUserId, int clubId, CreateInviteDto dto);
        Task<ApiResponse<string>> RevokeInviteAsync(int currentUserId, int inviteId);
        Task<ApiResponse<PagedResultDto<InviteResponseDto>>> GetClubInvitesAsync(int currentUserId, int clubId, PaginationParamsDto pagination, InviteStatus? status);
        Task<ApiResponse<InviteResponseDto>> GetInviteByIdAsync(int currentUserId, int inviteId);
        Task<ApiResponse<List<InviteResponseDto>>> GetMyInvitesAsync(int userId);
        Task<ApiResponse<string>> AcceptInviteAsync(int userId, int inviteId);
        Task<ApiResponse<string>> DeclineInviteAsync(int userId, int inviteId);
    }
}