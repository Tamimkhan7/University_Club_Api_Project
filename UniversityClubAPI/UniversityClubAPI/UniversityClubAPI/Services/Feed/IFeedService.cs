using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Feed;
using UniversityClubAPI.DTOs.Post;
using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.FeedService
{
    public interface IFeedService
    {
        Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetGlobalFeedAsync(int userId, int page, int pageSize);
        Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetPersonalizedFeedAsync(int userId, int page, int pageSize);
        Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetClubFeedAsync(int userId, int clubId, int page, int pageSize);
        Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetFollowingFeedAsync(int userId, int page, int pageSize);
        Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetSavedFeedAsync(int userId, int page, int pageSize);
        Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetUserFeedAsync(int viewerUserId, int targetUserId, int page, int pageSize);
        Task<ApiResponse<PagedResultDto<TrendingPostDto>>> GetTrendingAsync(int userId, int page, int pageSize);
        Task<ApiResponse<PagedResultDto<TrendingPostDto>>> GetMyClubsTrendingAsync(int userId, int page, int pageSize);
    }
}
