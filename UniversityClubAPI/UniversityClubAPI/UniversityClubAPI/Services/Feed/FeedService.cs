using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Feed;
using UniversityClubAPI.DTOs.Post;
using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.FeedService
{
    public class FeedService : IFeedService
    {
        private readonly AppDbContext _context;

        public FeedService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetGlobalFeedAsync(
            int userId, int page, int pageSize)
        {
            var blockedIds = await _context.GetBlockedUserIdsAsync(userId);

            var query = _context.Posts
                .AsNoTracking()
                .Where(p => !blockedIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new FeedItemDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserName = p.User != null ? p.User.Name : null,
                    UserImage = p.User != null ? p.User.ProfileImage : null,
                    ClubId = p.ClubId,
                    ClubName = p.Club != null ? p.Club.Name : null,
                    CommentCount = p.Comments.Count(),
                    ReactionCount = p.Reactions.Count(),
                    ShareCount = p.Shares.Count(),
                    IsSaved = p.SavedByUsers.Any(s => s.UserId == userId),
                    MyReaction = p.Reactions
                                     .Where(r => r.UserId == userId)
                                     .Select(r => r.Type.ToString())
                                     .FirstOrDefault()
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<FeedItemDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetPersonalizedFeedAsync(
            int userId, int page, int pageSize)
        {
            var blockedIds = await _context.GetBlockedUserIdsAsync(userId);

            var myClubIds = await _context.ClubMembers
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.ClubId)
                .ToListAsync();

            var followingIds = await _context.Follows
                .AsNoTracking()
                .Where(x => x.FollowerId == userId)
                .Select(x => x.FollowingId)
                .ToListAsync();

            var query = _context.Posts
                .AsNoTracking()
                .Where(p =>
                    !blockedIds.Contains(p.UserId) &&
                    (
                        myClubIds.Contains(p.ClubId) ||
                        followingIds.Contains(p.UserId) ||
                        p.UserId == userId
                    ))
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new FeedItemDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserName = p.User != null ? p.User.Name : null,
                    UserImage = p.User != null ? p.User.ProfileImage : null,
                    ClubId = p.ClubId,
                    ClubName = p.Club != null ? p.Club.Name : null,
                    CommentCount = p.Comments.Count(),
                    ReactionCount = p.Reactions.Count(),
                    ShareCount = p.Shares.Count(),
                    IsSaved = p.SavedByUsers.Any(s => s.UserId == userId),
                    MyReaction = p.Reactions
                                     .Where(r => r.UserId == userId)
                                     .Select(r => r.Type.ToString())
                                     .FirstOrDefault()
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<FeedItemDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetClubFeedAsync(
            int userId, int clubId, int page, int pageSize)
        {
            var clubExists = await _context.Clubs.AnyAsync(x => x.Id == clubId);
            if (!clubExists)
                throw new KeyNotFoundException("Club not found.");

            var isMember = await _context.IsMemberAsync(userId, clubId);
            if (!isMember)
                throw new UnauthorizedAccessException("You must be a club member to view this feed.");

            var blockedIds = await _context.GetBlockedUserIdsAsync(userId);

            var query = _context.Posts
                .AsNoTracking()
                .Where(p => p.ClubId == clubId && !blockedIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new FeedItemDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserName = p.User != null ? p.User.Name : null,
                    UserImage = p.User != null ? p.User.ProfileImage : null,
                    ClubId = p.ClubId,
                    ClubName = p.Club != null ? p.Club.Name : null,
                    CommentCount = p.Comments.Count(),
                    ReactionCount = p.Reactions.Count(),
                    ShareCount = p.Shares.Count(),
                    IsSaved = p.SavedByUsers.Any(s => s.UserId == userId),
                    MyReaction = p.Reactions
                                     .Where(r => r.UserId == userId)
                                     .Select(r => r.Type.ToString())
                                     .FirstOrDefault()
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<FeedItemDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetFollowingFeedAsync(
            int userId, int page, int pageSize)
        {
            var followingIds = await _context.Follows
                .AsNoTracking()
                .Where(x => x.FollowerId == userId)
                .Select(x => x.FollowingId)
                .ToListAsync();

            if (!followingIds.Any())
                return ApiResponse<PagedResultDto<FeedItemDto>>.Ok(
                    new PagedResultDto<FeedItemDto> { Page = page, PageSize = pageSize, TotalCount = 0, TotalPages = 0, Items = new() });

            var blockedIds = await _context.GetBlockedUserIdsAsync(userId);

            var query = _context.Posts
                .AsNoTracking()
                .Where(p => followingIds.Contains(p.UserId) && !blockedIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new FeedItemDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserName = p.User != null ? p.User.Name : null,
                    UserImage = p.User != null ? p.User.ProfileImage : null,
                    ClubId = p.ClubId,
                    ClubName = p.Club != null ? p.Club.Name : null,
                    CommentCount = p.Comments.Count(),
                    ReactionCount = p.Reactions.Count(),
                    ShareCount = p.Shares.Count(),
                    IsSaved = p.SavedByUsers.Any(s => s.UserId == userId),
                    MyReaction = p.Reactions
                                     .Where(r => r.UserId == userId)
                                     .Select(r => r.Type.ToString())
                                     .FirstOrDefault()
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<FeedItemDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetSavedFeedAsync(
            int userId, int page, int pageSize)
        {
            var query = _context.SavedPosts
                .AsNoTracking()
                .Where(sp => sp.UserId == userId)
                .OrderByDescending(sp => sp.SavedAt)
                .Select(sp => new FeedItemDto
                {
                    Id = sp.Post!.Id,
                    Content = sp.Post.Content,
                    ImageUrl = sp.Post.ImageUrl,
                    CreatedAt = sp.Post.CreatedAt,
                    UserId = sp.Post.UserId,
                    UserName = sp.Post.User != null ? sp.Post.User.Name : null,
                    UserImage = sp.Post.User != null ? sp.Post.User.ProfileImage : null,
                    ClubId = sp.Post.ClubId,
                    ClubName = sp.Post.Club != null ? sp.Post.Club.Name : null,
                    CommentCount = sp.Post.Comments.Count(),
                    ReactionCount = sp.Post.Reactions.Count(),
                    ShareCount = sp.Post.Shares.Count(),
                    IsSaved = true,
                    MyReaction = sp.Post.Reactions
                                     .Where(r => r.UserId == userId)
                                     .Select(r => r.Type.ToString())
                                     .FirstOrDefault()
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<FeedItemDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<FeedItemDto>>> GetUserFeedAsync(
            int viewerUserId, int targetUserId, int page, int pageSize)
        {
            await _context.EnsureUserExistsAsync(targetUserId);

            var isBlocked = await _context.BlockedUsers.AnyAsync(b =>
                (b.BlockerId == viewerUserId && b.BlockedUserId == targetUserId) ||
                (b.BlockerId == targetUserId && b.BlockedUserId == viewerUserId));

            if (isBlocked)
                throw new KeyNotFoundException("Content not available.");

            var query = _context.Posts
                .AsNoTracking()
                .Where(p => p.UserId == targetUserId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new FeedItemDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserName = p.User != null ? p.User.Name : null,
                    UserImage = p.User != null ? p.User.ProfileImage : null,
                    ClubId = p.ClubId,
                    ClubName = p.Club != null ? p.Club.Name : null,
                    CommentCount = p.Comments.Count(),
                    ReactionCount = p.Reactions.Count(),
                    ShareCount = p.Shares.Count(),
                    IsSaved = p.SavedByUsers.Any(s => s.UserId == viewerUserId),
                    MyReaction = p.Reactions
                                     .Where(r => r.UserId == viewerUserId)
                                     .Select(r => r.Type.ToString())
                                     .FirstOrDefault()
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<FeedItemDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<TrendingPostDto>>> GetTrendingAsync(
            int userId, int page, int pageSize)
        {
            var since = DateTime.UtcNow.AddHours(-24);
            var blockedIds = await _context.GetBlockedUserIdsAsync(userId);

            var query = _context.Posts
                .AsNoTracking()
                .Where(p => p.CreatedAt >= since && !blockedIds.Contains(p.UserId))
                .Select(p => new TrendingPostDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserName = p.User != null ? p.User.Name : null,
                    UserImage = p.User != null ? p.User.ProfileImage : null,
                    ClubId = p.ClubId,
                    ClubName = p.Club != null ? p.Club.Name : null,
                    CommentCount = p.Comments.Count(),
                    ReactionCount = p.Reactions.Count(),
                    ShareCount = p.Shares.Count(),
                    IsSaved = p.SavedByUsers.Any(s => s.UserId == userId),
                    MyReaction = p.Reactions
                                       .Where(r => r.UserId == userId)
                                       .Select(r => r.Type.ToString())
                                       .FirstOrDefault(),
                    TrendingScore = p.Reactions.Count() + p.Comments.Count() + p.Shares.Count()
                })
                .OrderByDescending(p => p.TrendingScore)
                .ThenByDescending(p => p.CreatedAt);

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<TrendingPostDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<TrendingPostDto>>> GetMyClubsTrendingAsync(
            int userId, int page, int pageSize)
        {
            var since = DateTime.UtcNow.AddHours(-24);

            var myClubIds = await _context.ClubMembers
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.ClubId)
                .ToListAsync();

            if (!myClubIds.Any())
                return ApiResponse<PagedResultDto<TrendingPostDto>>.Ok(
                    new PagedResultDto<TrendingPostDto> { Page = page, PageSize = pageSize, TotalCount = 0, TotalPages = 0, Items = new() });

            var blockedIds = await _context.GetBlockedUserIdsAsync(userId);

            var query = _context.Posts
                .AsNoTracking()
                .Where(p =>
                    p.CreatedAt >= since &&
                    !blockedIds.Contains(p.UserId) &&
                    myClubIds.Contains(p.ClubId))
                .Select(p => new TrendingPostDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserId = p.UserId,
                    UserName = p.User != null ? p.User.Name : null,
                    UserImage = p.User != null ? p.User.ProfileImage : null,
                    ClubId = p.ClubId,
                    ClubName = p.Club != null ? p.Club.Name : null,
                    CommentCount = p.Comments.Count(),
                    ReactionCount = p.Reactions.Count(),
                    ShareCount = p.Shares.Count(),
                    IsSaved = p.SavedByUsers.Any(s => s.UserId == userId),
                    MyReaction = p.Reactions
                                       .Where(r => r.UserId == userId)
                                       .Select(r => r.Type.ToString())
                                       .FirstOrDefault(),
                    TrendingScore = p.Reactions.Count() + p.Comments.Count() + p.Shares.Count()
                })
                .OrderByDescending(p => p.TrendingScore)
                .ThenByDescending(p => p.CreatedAt);

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<TrendingPostDto>>.Ok(result);
        }
    }
}