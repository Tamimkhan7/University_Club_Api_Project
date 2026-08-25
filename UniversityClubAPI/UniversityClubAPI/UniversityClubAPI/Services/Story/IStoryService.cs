using UniversityClubAPI.DTOs.Story;
using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.StoryService
{
    public interface IStoryService
    {
        Task<ApiResponse<StoryResponseDto>> CreateStoryAsync(int userId, CreateStoryDto dto);
        Task<ApiResponse<List<UserStoriesDto>>> GetFeedStoriesAsync(int currentUserId);
        Task<ApiResponse<List<StoryResponseDto>>> GetMyStoriesAsync(int userId);
        Task<ApiResponse<List<StoryResponseDto>>> GetUserStoriesAsync(int currentUserId, int targetUserId);
        Task<ApiResponse<string>> ViewStoryAsync(int userId, int storyId);
        Task<ApiResponse<List<StoryViewerDto>>> GetStoryViewersAsync(int userId, int storyId);
        Task<ApiResponse<string>> DeleteStoryAsync(int userId, int storyId);
    }
}
