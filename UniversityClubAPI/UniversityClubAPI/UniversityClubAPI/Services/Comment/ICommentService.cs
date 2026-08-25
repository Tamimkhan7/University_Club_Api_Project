using UniversityClubAPI.DTOs.Comment;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.Helpers;
namespace UniversityClubAPI.Services.CommentService
{
    public interface ICommentService
    {
        Task<ApiResponse<CommentDto>> CreateAsync(int userId, CreateCommentDto dto);
        Task<ApiResponse<string>> UpdateAsync(int userId, int id, CreateCommentDto dto);
        Task<ApiResponse<string>> DeleteAsync(int userId, int id);
        Task<ApiResponse<PagedResultDto<CommentDto>>> GetPostCommentsAsync(int userId, int postId, int page, int pageSize);
        Task<ApiResponse<CommentDto>> GetCommentByIdAsync(int userId, int id);
        Task<ApiResponse<List<CommentDto>>> GetRepliesAsync(int userId, int commentId);
        Task<ApiResponse<string>> ToggleLikeAsync(int userId, int commentId);
        Task<ApiResponse<int>> GetLikeCountAsync(int commentId);
    }
}