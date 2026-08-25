using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.DTOs.User;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ImageService _imageService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            AppDbContext context,
            ImageService imageService,
            INotificationService notificationService,
            ILogger<UserService> logger)
        {
            _context = context;
            _imageService = imageService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ApiResponse<UserProfileDto>> GetMyProfileAsync(int callerId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == callerId)
                .Select(x => new UserProfileDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    UserName = x.UserName,
                    Bio = x.Bio,
                    ProfileImage = x.ProfileImage,
                    CoverPhoto = x.CoverPhoto,
                    Department = x.Department,
                    Batch = x.Batch,
                    Role = x.Role,
                    IsPrivate = x.IsPrivate,
                    IsActive = x.IsActive,
                    ProfileViews = x.ProfileViews,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("User not found.");

            return ApiResponse<UserProfileDto>.Ok(user);
        }

        public async Task<ApiResponse<UserPublicDto>> GetProfileByIdAsync(int callerId, int targetUserId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == targetUserId)
                .Select(x => new UserPublicDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    UserName = x.UserName,
                    Bio = x.Bio,
                    ProfileImage = x.ProfileImage,
                    CoverPhoto = x.CoverPhoto,
                    Department = x.Department,
                    Batch = x.Batch,
                    Role = x.Role,
                    IsPrivate = x.IsPrivate,
                    CreatedAt = x.CreatedAt,
                    IsFollowing = _context.Follows.Any(f => f.FollowerId == callerId && f.FollowingId == targetUserId),
                    IsBlocked = _context.BlockedUsers.Any(b =>
                        (b.BlockerId == callerId && b.BlockedUserId == targetUserId) ||
                        (b.BlockerId == targetUserId && b.BlockedUserId == callerId))
                })
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("User not found.");

            return ApiResponse<UserPublicDto>.Ok(user);
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int callerId, UpdateUserDto dto)
        {
            var user = await _context.GetUserOrThrowAsync(callerId);

            if (!string.IsNullOrWhiteSpace(dto.Name)) user.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Bio)) user.Bio = dto.Bio;
            if (!string.IsNullOrWhiteSpace(dto.Department)) user.Department = dto.Department;
            if (!string.IsNullOrWhiteSpace(dto.Batch)) user.Batch = dto.Batch;
            if (!string.IsNullOrWhiteSpace(dto.UserName)) user.UserName = dto.UserName;

            if (dto.ProfileImage != null)
                user.ProfileImage = await _imageService.UploadImageAsync(dto.ProfileImage);
            if (dto.CoverPhoto != null)
                user.CoverPhoto = await _imageService.UploadImageAsync(dto.CoverPhoto);

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var updated = await GetMyProfileAsync(callerId);
            return ApiResponse<UserProfileDto>.Ok(updated.Data!, "Profile updated successfully");
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(int callerId, ChangePasswordDto dto)
        {
            var user = await _context.GetUserOrThrowAsync(callerId);

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
                throw new UnauthorizedAccessException("Current password is incorrect.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Password changed successfully");
        }

        public async Task<ApiResponse<string>> DeactivateAsync(int callerId)
        {
            var user = await _context.GetUserOrThrowAsync(callerId);
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Account deactivated");
        }

        public async Task<ApiResponse<string>> SoftDeleteAsync(int callerId)
        {
            var user = await _context.GetUserOrThrowAsync(callerId);
            user.IsDeleted = true;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Account deleted");
        }

        public async Task<ApiResponse<string>> SetPrivacyAsync(int callerId, bool isPrivate)
        {
            var user = await _context.GetUserOrThrowAsync(callerId);
            user.IsPrivate = isPrivate;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok($"Privacy set to {(isPrivate ? "private" : "public")}");
        }


        public async Task<ApiResponse<PagedResultDto<UserSummaryDto>>> GetAllAsync(int callerId, UserQueryDto query)
        {
            var q = _context.Users
                .AsNoTracking()
                .Where(x => !_context.BlockedUsers.Any(b =>
                    (b.BlockerId == callerId && b.BlockedUserId == x.Id) ||
                    (b.BlockerId == x.Id && b.BlockedUserId == callerId)));

            if (!string.IsNullOrWhiteSpace(query.Department))
                q = q.Where(x => x.Department == query.Department);
            if (!string.IsNullOrWhiteSpace(query.Batch))
                q = q.Where(x => x.Batch == query.Batch);

            q = q.OrderByDescending(x => x.CreatedAt);

            var result = await PaginationHelper.ToPagedResultAsync(
                q.Select(x => ToSummary(x, callerId)),
                query.Page,
                query.PageSize);

            return ApiResponse<PagedResultDto<UserSummaryDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<UserSummaryDto>>> SearchAsync(int callerId, UserQueryDto query)
        {
            if (string.IsNullOrWhiteSpace(query.Query))
                throw new ArgumentException("Search query cannot be empty.");

            var lower = query.Query.ToLower();
            var q = _context.Users
                .AsNoTracking()
                .Where(x =>
                    x.Id != callerId &&
                    !_context.BlockedUsers.Any(b =>
                        (b.BlockerId == callerId && b.BlockedUserId == x.Id) ||
                        (b.BlockerId == x.Id && b.BlockedUserId == callerId)) &&
                    (
                        x.Name.ToLower().Contains(lower) ||
                        (x.UserName != null && x.UserName.ToLower().Contains(lower)) ||
                        x.Email.ToLower().Contains(lower) ||
                        (x.Department != null && x.Department.ToLower().Contains(lower)) ||
                        (x.Batch != null && x.Batch.ToLower().Contains(lower))
                    ))
                .OrderByDescending(x => x.CreatedAt);

            var result = await PaginationHelper.ToPagedResultAsync(
                q.Select(x => ToSummary(x, callerId)),
                query.Page,
                query.PageSize);

            return ApiResponse<PagedResultDto<UserSummaryDto>>.Ok(result);
        }


        public async Task<ApiResponse<string>> FollowAsync(int callerId, int targetUserId)
        {
            if (callerId == targetUserId)
                throw new InvalidOperationException("You cannot follow yourself.");

            await _context.EnsureUserExistsAsync(targetUserId);

            var isBlocked = await _context.BlockedUsers.AnyAsync(x =>
                (x.BlockerId == callerId && x.BlockedUserId == targetUserId) ||
                (x.BlockerId == targetUserId && x.BlockedUserId == callerId));
            if (isBlocked)
                throw new InvalidOperationException("Cannot follow this user.");

            var alreadyFollowing = await _context.Follows
                .AnyAsync(x => x.FollowerId == callerId && x.FollowingId == targetUserId);
            if (alreadyFollowing)
                throw new InvalidOperationException("Already following this user.");

            _context.Follows.Add(new Follow
            {
                FollowerId = callerId,
                FollowingId = targetUserId
            });

            await _context.SaveChangesAsync();

            var callerName = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == callerId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync() ?? "Someone";

            _ = Task.Run(async () =>
            {
                try
                {
                    await _notificationService.CreateAndPushAsync(new CreateNotificationDto
                    {
                        SenderId = callerId,
                        ReceiverId = targetUserId,
                        Type = NotificationType.Follow,
                        Message = $"{callerName} started following you."
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to send Follow notification to user {UserId}", targetUserId);
                }
            });

            return ApiResponse<string>.Ok("Followed successfully");
        }

        public async Task<ApiResponse<string>> UnfollowAsync(int callerId, int targetUserId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(x => x.FollowerId == callerId && x.FollowingId == targetUserId)
                ?? throw new KeyNotFoundException("Follow relationship not found.");

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Unfollowed successfully");
        }

        public async Task<ApiResponse<PagedResultDto<UserSummaryDto>>> GetFollowersAsync(
            int callerId,
            int targetUserId,
            PaginationParamsDto pagination)
        {
            var q = _context.Follows
                .AsNoTracking()
                .Where(x => x.FollowingId == targetUserId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => ToSummary(x.Follower!, callerId));

            var result = await PaginationHelper.ToPagedResultAsync(q, pagination);
            return ApiResponse<PagedResultDto<UserSummaryDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<UserSummaryDto>>> GetFollowingAsync(
            int callerId,
            int targetUserId,
            PaginationParamsDto pagination)
        {
            var q = _context.Follows
                .AsNoTracking()
                .Where(x => x.FollowerId == targetUserId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => ToSummary(x.Following!, callerId));

            var result = await PaginationHelper.ToPagedResultAsync(q, pagination);
            return ApiResponse<PagedResultDto<UserSummaryDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<UserSummaryDto>>> GetMutualFollowsAsync(
            int callerId,
            int targetUserId,
            PaginationParamsDto pagination)
        {
            var q = _context.Follows
                .AsNoTracking()
                .Where(x => x.FollowerId == callerId)
                .Where(x => _context.Follows.Any(y => y.FollowerId == targetUserId && y.FollowingId == x.FollowingId))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => ToSummary(x.Following!, callerId));

            var result = await PaginationHelper.ToPagedResultAsync(q, pagination);
            return ApiResponse<PagedResultDto<UserSummaryDto>>.Ok(result);
        }


        public async Task<ApiResponse<string>> BlockAsync(int callerId, int targetUserId)
        {
            if (callerId == targetUserId)
                throw new InvalidOperationException("You cannot block yourself.");

            await _context.EnsureUserExistsAsync(targetUserId);

            var alreadyBlocked = await _context.BlockedUsers
                .AnyAsync(x => x.BlockerId == callerId && x.BlockedUserId == targetUserId);
            if (alreadyBlocked)
                throw new InvalidOperationException("User is already blocked.");

            var follows = await _context.Follows
                .Where(x =>
                    (x.FollowerId == callerId && x.FollowingId == targetUserId) ||
                    (x.FollowerId == targetUserId && x.FollowingId == callerId))
                .ToListAsync();

            if (follows.Count > 0)
                _context.Follows.RemoveRange(follows);

            _context.BlockedUsers.Add(new BlockedUser
            {
                BlockerId = callerId,
                BlockedUserId = targetUserId
            });

            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("User blocked");
        }

        public async Task<ApiResponse<string>> UnblockAsync(int callerId, int targetUserId)
        {
            var block = await _context.BlockedUsers
                .FirstOrDefaultAsync(x => x.BlockerId == callerId && x.BlockedUserId == targetUserId)
                ?? throw new KeyNotFoundException("Block not found.");

            _context.BlockedUsers.Remove(block);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("User unblocked");
        }

        public async Task<ApiResponse<PagedResultDto<UserSummaryDto>>> GetBlockedAsync(
            int callerId,
            PaginationParamsDto pagination)
        {
            var q = _context.BlockedUsers
                .AsNoTracking()
                .Where(x => x.BlockerId == callerId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new UserSummaryDto
                {
                    Id = x.BlockedUserInfo!.Id,
                    Name = x.BlockedUserInfo.Name,
                    UserName = x.BlockedUserInfo.UserName,
                    ProfileImage = x.BlockedUserInfo.ProfileImage,
                    Department = x.BlockedUserInfo.Department,
                    Batch = x.BlockedUserInfo.Batch,
                    IsFollowing = false
                });

            var result = await PaginationHelper.ToPagedResultAsync(q, pagination);
            return ApiResponse<PagedResultDto<UserSummaryDto>>.Ok(result);
        }


        public async Task<ApiResponse<UserStatsDto>> GetStatsAsync(int targetUserId)
        {
            await _context.EnsureUserExistsAsync(targetUserId);

            var followers = await _context.Follows.CountAsync(x => x.FollowingId == targetUserId);
            var following = await _context.Follows.CountAsync(x => x.FollowerId == targetUserId);
            var posts = await _context.Posts.CountAsync(x => x.UserId == targetUserId);
            var profileViews = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == targetUserId)
                .Select(x => x.ProfileViews)
                .FirstOrDefaultAsync();

            return ApiResponse<UserStatsDto>.Ok(new UserStatsDto
            {
                UserId = targetUserId,
                Followers = followers,
                Following = following,
                Posts = posts,
                ProfileViews = profileViews
            });
        }

        public async Task<ApiResponse<string>> RecordProfileViewAsync(int callerId, int targetUserId)
        {
            if (callerId == targetUserId)
                return ApiResponse<string>.Ok("Profile view recorded");

            var today = DateTime.UtcNow.Date;
            var alreadyViewed = await _context.ProfileViews.AnyAsync(x =>
                x.ViewerId == callerId &&
                x.ProfileOwnerId == targetUserId &&
                x.ViewedAt >= today);

            if (alreadyViewed)
                return ApiResponse<string>.Ok("Profile view recorded");

            _context.ProfileViews.Add(new ProfileView
            {
                ViewerId = callerId,
                ProfileOwnerId = targetUserId,
                ViewedAt = DateTime.UtcNow
            });

            await _context.Users
                .Where(x => x.Id == targetUserId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.ProfileViews, u => u.ProfileViews + 1));

            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Profile view recorded");
        }


        private static UserSummaryDto ToSummary(Models.User x, int callerId) => new()
        {
            Id = x.Id,
            Name = x.Name,
            UserName = x.UserName,
            ProfileImage = x.ProfileImage,
            Department = x.Department,
            Batch = x.Batch,
            IsFollowing = false
        };
    }
}