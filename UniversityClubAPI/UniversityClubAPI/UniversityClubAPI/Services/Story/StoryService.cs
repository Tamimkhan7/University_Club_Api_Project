using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Story;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Services.StoryService
{
    public class StoryService : IStoryService
    {
        private readonly AppDbContext _context;
        private readonly ImageService _imageService;
        private readonly ILogger<StoryService> _logger;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".mov", ".webm" };
        private const long MaxStorySizeBytes = 25 * 1024 * 1024;

        public StoryService(AppDbContext context, ImageService imageService, ILogger<StoryService> logger)
        {
            _context = context;
            _imageService = imageService;
            _logger = logger;
        }

        private static StoryResponseDto ToDto(Story s, int currentUserId) => new()
        {
            Id = s.Id,
            UserId = s.UserId,
            UserName = s.User?.Name,
            UserProfileImage = s.User?.ProfileImage,
            MediaUrl = s.MediaUrl,
            MediaType = s.MediaType,
            Caption = s.Caption,
            CreatedAt = s.CreatedAt,
            ExpiresAt = s.ExpiresAt,
            ViewCount = s.Views.Count,
            ViewedByMe = s.Views.Any(v => v.ViewerId == currentUserId)
        };

        private IQueryable<Story> ActiveStoriesQuery()
        {
            var now = DateTime.UtcNow;
            return _context.Stories
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Views)
                .Where(x => x.ExpiresAt > now);
        }

        private async Task<HashSet<int>> GetBlockedRelationUserIdsAsync(int currentUserId)
        {
            var blockedByMe = _context.BlockedUsers
                .Where(x => x.BlockerId == currentUserId)
                .Select(x => x.BlockedUserId);

            var blockedMe = _context.BlockedUsers
                .Where(x => x.BlockedUserId == currentUserId)
                .Select(x => x.BlockerId);

            var ids = await blockedByMe.Union(blockedMe).ToListAsync();
            return ids.ToHashSet();
        }

        public async Task<ApiResponse<StoryResponseDto>> CreateStoryAsync(int userId, CreateStoryDto dto)
        {
            if (dto.Media == null || dto.Media.Length == 0)
                return ApiResponse<StoryResponseDto>.Fail("Media file is required.");

            if (dto.Media.Length > MaxStorySizeBytes)
                return ApiResponse<StoryResponseDto>.Fail("File size must be less than 25 MB.");

            var extension = Path.GetExtension(dto.Media.FileName).ToLowerInvariant();

            string mediaUrl;
            StoryMediaType mediaType;

            if (AllowedImageExtensions.Contains(extension))
            {
                mediaType = StoryMediaType.Image;
                var url = await _imageService.UploadImageAsync(dto.Media);
                if (url == null)
                    return ApiResponse<StoryResponseDto>.Fail("Failed to upload image.");
                mediaUrl = url;
            }
            else if (AllowedVideoExtensions.Contains(extension))
            {
                mediaType = StoryMediaType.Video;
                var url = await _imageService.UploadVideoAsync(dto.Media);
                if (url == null)
                    return ApiResponse<StoryResponseDto>.Fail("Failed to upload video.");
                mediaUrl = url;
            }
            else
            {
                return ApiResponse<StoryResponseDto>.Fail(
                    "Unsupported file type. Allowed: jpg, jpeg, png, mp4, mov, webm.");
            }

            var now = DateTime.UtcNow;
            var story = new Story
            {
                UserId = userId,
                MediaUrl = mediaUrl,
                MediaType = mediaType,
                Caption = dto.Caption,
                CreatedAt = now,
                ExpiresAt = now.AddHours(24)
            };

            _context.Stories.Add(story);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} created a {MediaType} story {StoryId}", userId, mediaType, story.Id);

            var result = await _context.Stories
                .Include(x => x.User)
                .Include(x => x.Views)
                .FirstAsync(x => x.Id == story.Id);

            return ApiResponse<StoryResponseDto>.Ok(ToDto(result, userId), "Story posted successfully.");
        }

        public async Task<ApiResponse<List<UserStoriesDto>>> GetFeedStoriesAsync(int currentUserId)
        {
            var followingIds = await _context.Follows
                .Where(x => x.FollowerId == currentUserId)
                .Select(x => x.FollowingId)
                .ToListAsync();

            followingIds.Add(currentUserId);

            var blockedRelationIds = await GetBlockedRelationUserIdsAsync(currentUserId);
            if (blockedRelationIds.Count > 0)
                followingIds = followingIds.Where(id => !blockedRelationIds.Contains(id)).ToList();

            var stories = await ActiveStoriesQuery()
                .Where(x => followingIds.Contains(x.UserId))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var grouped = stories
                .GroupBy(x => x.UserId)
                .Select(g => new UserStoriesDto
                {
                    UserId = g.Key,
                    UserName = g.First().User?.Name,
                    UserProfileImage = g.First().User?.ProfileImage,
                    HasUnviewed = g.Any(s => !s.Views.Any(v => v.ViewerId == currentUserId)),
                    Stories = g.OrderBy(s => s.CreatedAt).Select(s => ToDto(s, currentUserId)).ToList()
                })

                .OrderByDescending(g => g.UserId == currentUserId)
                .ThenByDescending(g => g.HasUnviewed)
                .ThenByDescending(g => g.Stories.Max(s => s.CreatedAt))
                .ToList();

            return ApiResponse<List<UserStoriesDto>>.Ok(grouped);
        }

        public async Task<ApiResponse<List<StoryResponseDto>>> GetMyStoriesAsync(int userId)
        {
            var stories = await ActiveStoriesQuery()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return ApiResponse<List<StoryResponseDto>>.Ok(stories.Select(s => ToDto(s, userId)).ToList());
        }

        public async Task<ApiResponse<List<StoryResponseDto>>> GetUserStoriesAsync(int currentUserId, int targetUserId)
        {
            var targetExists = await _context.Users.AnyAsync(x => x.Id == targetUserId);
            if (!targetExists)
                return ApiResponse<List<StoryResponseDto>>.Fail("User not found.");


            if (currentUserId != targetUserId)
            {
                var isBlockedEitherWay = await _context.BlockedUsers.AnyAsync(x =>
                    (x.BlockerId == currentUserId && x.BlockedUserId == targetUserId) ||
                    (x.BlockerId == targetUserId && x.BlockedUserId == currentUserId));

                if (isBlockedEitherWay)
                    return ApiResponse<List<StoryResponseDto>>.Fail("User not found.");
            }

            var stories = await ActiveStoriesQuery()
                .Where(x => x.UserId == targetUserId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            return ApiResponse<List<StoryResponseDto>>.Ok(stories.Select(s => ToDto(s, currentUserId)).ToList());
        }

        public async Task<ApiResponse<string>> ViewStoryAsync(int userId, int storyId)
        {
            var story = await _context.Stories.FirstOrDefaultAsync(x => x.Id == storyId);
            if (story == null)
                return ApiResponse<string>.Fail("Story not found.");

            if (story.ExpiresAt <= DateTime.UtcNow)
                return ApiResponse<string>.Fail("This story has expired.");

            if (story.UserId == userId)
                return ApiResponse<string>.Ok("This is your own story.");


            var isBlockedEitherWay = await _context.BlockedUsers.AnyAsync(x =>
                (x.BlockerId == userId && x.BlockedUserId == story.UserId) ||
                (x.BlockerId == story.UserId && x.BlockedUserId == userId));

            if (isBlockedEitherWay)
                return ApiResponse<string>.Fail("Story not found.");

            var alreadyViewed = await _context.StoryViews
                .AnyAsync(x => x.StoryId == storyId && x.ViewerId == userId);

            if (!alreadyViewed)
            {
                _context.StoryViews.Add(new StoryView
                {
                    StoryId = storyId,
                    ViewerId = userId
                });
                await _context.SaveChangesAsync();
            }

            return ApiResponse<string>.Ok("Story marked as viewed.");
        }

        public async Task<ApiResponse<List<StoryViewerDto>>> GetStoryViewersAsync(int userId, int storyId)
        {
            var story = await _context.Stories.FirstOrDefaultAsync(x => x.Id == storyId);
            if (story == null)
                return ApiResponse<List<StoryViewerDto>>.Fail("Story not found.");

            if (story.UserId != userId)
                return ApiResponse<List<StoryViewerDto>>.Fail("Only the story owner can see its viewers.");

            var viewers = await _context.StoryViews
                .AsNoTracking()
                .Include(x => x.Viewer)
                .Where(x => x.StoryId == storyId)
                .OrderByDescending(x => x.ViewedAt)
                .Select(x => new StoryViewerDto
                {
                    UserId = x.ViewerId,
                    UserName = x.Viewer!.Name,
                    UserProfileImage = x.Viewer.ProfileImage,
                    ViewedAt = x.ViewedAt
                })
                .ToListAsync();

            return ApiResponse<List<StoryViewerDto>>.Ok(viewers);
        }

        public async Task<ApiResponse<string>> DeleteStoryAsync(int userId, int storyId)
        {
            var story = await _context.Stories.FirstOrDefaultAsync(x => x.Id == storyId);
            if (story == null)
                return ApiResponse<string>.Fail("Story not found.");

            if (story.UserId != userId)
                return ApiResponse<string>.Fail("You can only delete your own story.");

            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Story deleted successfully.");
        }
    }
}