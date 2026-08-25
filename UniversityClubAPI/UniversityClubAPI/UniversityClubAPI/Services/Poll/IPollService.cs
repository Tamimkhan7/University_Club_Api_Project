using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Poll;
using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.PollService
{
    public interface IPollService
    {
        Task<ApiResponse<PollResponseDto>> CreatePollAsync(int userId, int clubId, CreatePollDto dto);
        Task<ApiResponse<PagedResultDto<PollResponseDto>>> GetClubPollsAsync(int userId, int clubId, bool activeOnly, PaginationParamsDto pagination);
        Task<ApiResponse<PollResponseDto>> GetPollByIdAsync(int userId, int pollId);
        Task<ApiResponse<PollResponseDto>> VoteAsync(int userId, int pollId, CastVoteDto dto);
        Task<ApiResponse<string>> ClosePollAsync(int userId, int pollId);
        Task<ApiResponse<string>> DeletePollAsync(int userId, int pollId);
    }
}
