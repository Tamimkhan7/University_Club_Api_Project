using UniversityClubAPI.DTOs;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Group;
using UniversityClubAPI.DTOs.GroupMessage;
using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.GroupService
{
    public interface IGroupService
    {
        Task<ApiResponse<GroupSummaryDto>> CreateAsync(int userId, CreateGroupDto dto);
        Task<ApiResponse<GroupSummaryDto>> UpdateAsync(int userId, int groupId, UpdateGroupDto dto);
        Task<ApiResponse<string>> DeleteGroupAsync(int userId, int groupId);

        Task<ApiResponse<GroupMessageDto>> SendMessageAsync(int userId, SendGroupMessageDto dto);
        Task<ApiResponse<PagedResultDto<GroupMessageDto>>> GetMessagesAsync(int userId, int groupId, PaginationParamsDto pagination);

        Task<ApiResponse<string>> LeaveGroupAsync(int userId, int groupId);
        Task<ApiResponse<string>> AddMemberAsync(int userId, int groupId, AddGroupMemberDto dto);
        Task<ApiResponse<string>> RemoveMemberAsync(int userId, int groupId, int memberId);
        Task<ApiResponse<string>> SetAdminAsync(int userId, int groupId, SetGroupAdminDto dto);
        Task<ApiResponse<List<GroupMemberDto>>> GetMembersAsync(int userId, int groupId);

        Task<ApiResponse<List<GroupSummaryDto>>> GetMyGroupsAsync(int userId);
        Task<ApiResponse<GroupDetailsDto>> GetByIdAsync(int userId, int groupId);
    }
}
