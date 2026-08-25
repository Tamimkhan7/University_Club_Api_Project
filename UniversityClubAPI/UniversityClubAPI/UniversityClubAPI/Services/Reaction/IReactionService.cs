using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Reaction;
using UniversityClubAPI.Enums;

namespace UniversityClubAPI.Services.ReactionService
{
    public interface IReactionService
    {

        Task<ReactionSummaryDto> ReactAsync(int callerId, ReactDto dto);
        Task<ReactionSummaryDto> RemoveAsync(int callerId, int postId);
        Task<ReactionSummaryDto> GetSummaryAsync(int callerId, int postId);
        Task<int> GetCountAsync(int postId);
        Task<ReactionType?> GetMyReactionAsync(int callerId, int postId);
        Task<PagedResultDto<ReactionResponseDto>> GetAllAsync(int postId, PaginationParamsDto pagination);
        Task<PagedResultDto<ReactionUserDto>> GetByTypeAsync(int postId, ReactionType type, PaginationParamsDto pagination);
    }
}
