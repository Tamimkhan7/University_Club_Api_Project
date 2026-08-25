using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Post;

namespace UniversityClubAPI.Services.PostService
{
    public interface IPostService
    {
        Task<PostResponseDto> CreateAsync(int callerId, CreatePostDto dto);
        Task<PostResponseDto> UpdateAsync(int callerId, int postId, UpdatePostDto dto);
        Task DeleteAsync(int callerId, int postId);
        Task<PagedResultDto<PostResponseDto>> GetAllAsync(int callerId, PostQueryDto query);
        Task<PostResponseDto> GetByIdAsync(int callerId, int postId);
        Task SavePostAsync(int callerId, int postId);
        Task UnsavePostAsync(int callerId, int postId);
        Task<PagedResultDto<PostResponseDto>> GetSavedAsync(int callerId, PaginationParamsDto pagination);
        Task<PagedResultDto<PostResponseDto>> SearchAsync(int callerId, string query, PaginationParamsDto pagination);
        Task ReportAsync(int callerId, ReportPostDto dto);
    }
}
