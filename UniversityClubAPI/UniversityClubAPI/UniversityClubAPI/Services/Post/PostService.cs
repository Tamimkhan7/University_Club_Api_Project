using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.DTOs.Post;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Services.PostService
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _context;
        private readonly ImageService _imageService;
        private readonly INotificationService _notificationService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PostService> _logger;

        public PostService(
            AppDbContext context,
            ImageService imageService,
            INotificationService notificationService,
            IServiceScopeFactory scopeFactory,
            ILogger<PostService> logger)
        {
            _context = context;
            _imageService = imageService;
            _notificationService = notificationService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<PostResponseDto> CreateAsync(int callerId, CreatePostDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content) && dto.Image == null)
                throw new ArgumentException("Post must have content or an image.");

            var club = await _context.Clubs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.ClubId)
                ?? throw new KeyNotFoundException("Club not found.");

            var membership = await _context.ClubMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ClubId == dto.ClubId && x.UserId == callerId)
                ?? throw new UnauthorizedAccessException("You must join the club before posting.");

            if (!ClubPermissionHelper.CanManage(membership.Role) && !membership.IsApproved)
            {
                throw new UnauthorizedAccessException("Your membership is not yet approved.");
            }

            string? imageUrl = null;
            if (dto.Image != null)
                imageUrl = await _imageService.UploadImageAsync(dto.Image);

            var post = new Post
            {
                Content = dto.Content,
                ImageUrl = imageUrl,
                ClubId = dto.ClubId,
                UserId = callerId
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var authorName = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == callerId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync() ?? "Someone";

            var memberIds = await _context.ClubMembers
                .AsNoTracking()
                .Where(x => x.ClubId == dto.ClubId && x.UserId != callerId)
                .Select(x => x.UserId)
                .ToListAsync();

            var clubName = club.Name;


            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                foreach (var memberId in memberIds)
                {
                    try
                    {
                        await notificationService.CreateAndPushAsync(new CreateNotificationDto
                        {
                            SenderId = callerId,
                            ReceiverId = memberId,
                            Type = NotificationType.NewPost,
                            Message = $"{authorName} posted in {clubName}."
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to send NewPost notification to member {MemberId}", memberId);
                    }
                }
            });

            return await BuildDtoAsync(post.Id, callerId);
        }

        public async Task<PostResponseDto> UpdateAsync(int callerId, int postId, UpdatePostDto dto)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(x => x.Id == postId && x.UserId == callerId)
                ?? throw new KeyNotFoundException("Post not found or you are not the author.");

            if (!string.IsNullOrWhiteSpace(dto.Content))
                post.Content = dto.Content;

            if (dto.Image != null)
                post.ImageUrl = await _imageService.UploadImageAsync(dto.Image);

            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await BuildDtoAsync(post.Id, callerId);
        }

        public async Task DeleteAsync(int callerId, int postId)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(x => x.Id == postId)
                ?? throw new KeyNotFoundException("Post not found.");
            if (post.UserId != callerId)
            {
                var membership = await _context.ClubMembers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.ClubId == post.ClubId &&
                        x.UserId == callerId &&
                        ClubPermissionHelper.CanManage(x.Role));

                if (membership == null)
                    throw new UnauthorizedAccessException("You are not allowed to delete this post.");
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResultDto<PostResponseDto>> GetAllAsync(int callerId, PostQueryDto query)
        {
            var q = _context.Posts.AsNoTracking();

            if (query.ClubId.HasValue)
                q = q.Where(x => x.ClubId == query.ClubId.Value);

            if (query.UserId.HasValue)
                q = q.Where(x => x.UserId == query.UserId.Value);

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                var lower = query.Query.ToLower();
                q = q.Where(x => x.Content != null && x.Content.ToLower().Contains(lower));
            }

            q = q.OrderByDescending(x => x.CreatedAt);

            return await PaginationHelper.ToPagedResultAsync(
                q.Select(x => new PostResponseDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    ImageUrl = x.ImageUrl,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    UserId = x.UserId,
                    UserName = x.User!.Name,
                    UserImage = x.User.ProfileImage,
                    ClubId = x.ClubId,
                    ClubName = x.Club!.Name,
                    CommentCount = x.Comments.Count,
                    ReactionCount = x.Reactions.Count,
                    ShareCount = x.Shares.Count,
                    SaveCount = x.SavedByUsers.Count,
                    IsLiked = x.Reactions.Any(r => r.UserId == callerId),
                    IsSaved = x.SavedByUsers.Any(s => s.UserId == callerId)
                }),
                query.Page,
                query.PageSize);
        }

        public async Task<PostResponseDto> GetByIdAsync(int callerId, int postId)
        {
            var dto = await _context.Posts
                .AsNoTracking()
                .Where(x => x.Id == postId)
                .Select(x => new PostResponseDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    ImageUrl = x.ImageUrl,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    UserId = x.UserId,
                    UserName = x.User!.Name,
                    UserImage = x.User.ProfileImage,
                    ClubId = x.ClubId,
                    ClubName = x.Club!.Name,
                    CommentCount = x.Comments.Count,
                    ReactionCount = x.Reactions.Count,
                    ShareCount = x.Shares.Count,
                    SaveCount = x.SavedByUsers.Count,
                    IsLiked = x.Reactions.Any(r => r.UserId == callerId),
                    IsSaved = x.SavedByUsers.Any(s => s.UserId == callerId)
                })
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("Post not found.");

            return dto;
        }

        public async Task SavePostAsync(int callerId, int postId)
        {
            var postExists = await _context.Posts.AnyAsync(x => x.Id == postId);
            if (!postExists)
                throw new KeyNotFoundException("Post not found.");

            var alreadySaved = await _context.SavedPosts
                .AnyAsync(x => x.PostId == postId && x.UserId == callerId);

            if (alreadySaved)
                throw new InvalidOperationException("Post is already saved.");

            _context.SavedPosts.Add(new SavedPost
            {
                UserId = callerId,
                PostId = postId
            });

            await _context.SaveChangesAsync();
        }

        public async Task UnsavePostAsync(int callerId, int postId)
        {
            var savedPost = await _context.SavedPosts
                .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == callerId)
                ?? throw new KeyNotFoundException("Saved post not found.");

            _context.SavedPosts.Remove(savedPost);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResultDto<PostResponseDto>> GetSavedAsync(
            int callerId,
            PaginationParamsDto pagination)
        {
            var q = _context.SavedPosts
                .AsNoTracking()
                .Where(x => x.UserId == callerId)
                .OrderByDescending(x => x.SavedAt)
                .Select(x => new PostResponseDto
                {
                    Id = x.Post!.Id,
                    Content = x.Post.Content,
                    ImageUrl = x.Post.ImageUrl,
                    CreatedAt = x.Post.CreatedAt,
                    UpdatedAt = x.Post.UpdatedAt,
                    UserId = x.Post.UserId,
                    UserName = x.Post.User!.Name,
                    UserImage = x.Post.User.ProfileImage,
                    ClubId = x.Post.ClubId,
                    ClubName = x.Post.Club!.Name,
                    CommentCount = x.Post.Comments.Count,
                    ReactionCount = x.Post.Reactions.Count,
                    ShareCount = x.Post.Shares.Count,
                    SaveCount = x.Post.SavedByUsers.Count,
                    IsLiked = x.Post.Reactions.Any(r => r.UserId == callerId),
                    IsSaved = true
                });

            return await PaginationHelper.ToPagedResultAsync(q, pagination);
        }

        public async Task<PagedResultDto<PostResponseDto>> SearchAsync(
            int callerId,
            string query,
            PaginationParamsDto pagination)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query cannot be empty.");

            var lower = query.ToLower();

            var q = _context.Posts
                .AsNoTracking()
                .Where(x => x.Content != null && x.Content.ToLower().Contains(lower))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new PostResponseDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    ImageUrl = x.ImageUrl,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    UserId = x.UserId,
                    UserName = x.User!.Name,
                    UserImage = x.User.ProfileImage,
                    ClubId = x.ClubId,
                    ClubName = x.Club!.Name,
                    CommentCount = x.Comments.Count,
                    ReactionCount = x.Reactions.Count,
                    ShareCount = x.Shares.Count,
                    SaveCount = x.SavedByUsers.Count,
                    IsLiked = x.Reactions.Any(r => r.UserId == callerId),
                    IsSaved = x.SavedByUsers.Any(s => s.UserId == callerId)
                });

            return await PaginationHelper.ToPagedResultAsync(q, pagination);
        }

        public async Task ReportAsync(int callerId, ReportPostDto dto)
        {
            var postExists = await _context.Posts.AnyAsync(x => x.Id == dto.PostId);
            if (!postExists)
                throw new KeyNotFoundException("Post not found.");

            var alreadyReported = await _context.PostReports
                .AnyAsync(x => x.PostId == dto.PostId && x.ReporterId == callerId);

            if (alreadyReported)
                throw new InvalidOperationException("You have already reported this post.");

            _context.PostReports.Add(new PostReport
            {
                ReporterId = callerId,
                PostId = dto.PostId,
                Reason = dto.Reason
            });

            await _context.SaveChangesAsync();
        }

        private async Task<PostResponseDto> BuildDtoAsync(int postId, int callerId)
        {
            return await _context.Posts
                .AsNoTracking()
                .Where(x => x.Id == postId)
                .Select(x => new PostResponseDto
                {
                    Id = x.Id,
                    Content = x.Content,
                    ImageUrl = x.ImageUrl,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    UserId = x.UserId,
                    UserName = x.User!.Name,
                    UserImage = x.User.ProfileImage,
                    ClubId = x.ClubId,
                    ClubName = x.Club!.Name,
                    CommentCount = x.Comments.Count,
                    ReactionCount = x.Reactions.Count,
                    ShareCount = x.Shares.Count,
                    SaveCount = x.SavedByUsers.Count,
                    IsLiked = x.Reactions.Any(r => r.UserId == callerId),
                    IsSaved = x.SavedByUsers.Any(s => s.UserId == callerId)
                })
                .FirstAsync();
        }
    }
}