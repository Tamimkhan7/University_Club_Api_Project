using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Follow;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Services.FollowService
{
    public class FollowService : IFollowService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FollowService> _logger;
        private readonly INotificationService _notificationService;

        public FollowService(AppDbContext context, ILogger<FollowService> logger, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }
        private async Task<List<int>> GetBlockedIdsAsync(int userId)
            => await _context.BlockedUsers
                .Where(x => x.BlockerId == userId || x.BlockedUserId == userId)
                .Select(x => x.BlockerId == userId ? x.BlockedUserId : x.BlockerId)
                .ToListAsync();

        private async Task<List<int>> GetFollowingIdsAsync(int userId)
            => await _context.Follows
                .Where(x => x.FollowerId == userId)
                .Select(x => x.FollowingId)
                .ToListAsync();

        private async Task<bool> IsBlockedAsync(int a, int b)
            => await _context.BlockedUsers
                .AnyAsync(x =>
                    (x.BlockerId == a && x.BlockedUserId == b) ||
                    (x.BlockerId == b && x.BlockedUserId == a));

        public async Task<ApiResponse<string>> FollowAsync(int currentUserId, int targetUserId)
        {
            if (currentUserId == targetUserId)
                throw new ArgumentException("You cannot follow yourself.");

            await _context.EnsureUserExistsAsync(targetUserId);

            if (await IsBlockedAsync(currentUserId, targetUserId))
                throw new UnauthorizedAccessException("Follow not allowed.");

            if (await _context.Follows.AnyAsync(x => x.FollowerId == currentUserId && x.FollowingId == targetUserId))
                throw new ArgumentException("You are already following this user.");

            _context.Follows.Add(new Follow
            {
                FollowerId = currentUserId,
                FollowingId = targetUserId
            });

            await _notificationService.CreateAndPushAsync(new CreateNotificationDto
            {
                SenderId = currentUserId,
                ReceiverId = targetUserId,
                Type = NotificationType.Follow,
                Message = "Started following you"
            });

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {A} followed User {B}", currentUserId, targetUserId);
            return ApiResponse<string>.Ok("User followed successfully.");
        }

        public async Task<ApiResponse<string>> UnfollowAsync(int currentUserId, int targetUserId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(x => x.FollowerId == currentUserId && x.FollowingId == targetUserId);

            if (follow == null)
                throw new ArgumentException("You are not following this user.");

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {A} unfollowed User {B}", currentUserId, targetUserId);
            return ApiResponse<string>.Ok("User unfollowed successfully.");
        }

        public async Task<ApiResponse<PagedResultDto<FollowUserDto>>> GetMyFollowersAsync(
            int currentUserId, PaginationParamsDto pagination)
        {
            var blockedIds = await GetBlockedIdsAsync(currentUserId);

            var query = _context.Follows
                .AsNoTracking()
                .Where(x => x.FollowingId == currentUserId && !blockedIds.Contains(x.FollowerId))
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Select(x => new FollowUserDto
                {
                    Id = x.Follower!.Id,
                    Name = x.Follower.Name,
                    Email = x.Follower.Email,
                    ProfileImage = x.Follower.ProfileImage,
                    FollowedAt = x.CreatedAt
                });

            return ApiResponse<PagedResultDto<FollowUserDto>>.Ok(
                await PaginationHelper.ToPagedResultAsync(query, pagination));
        }

        public async Task<ApiResponse<PagedResultDto<FollowUserDto>>> GetMyFollowingAsync(
            int currentUserId, PaginationParamsDto pagination)
        {
            var blockedIds = await GetBlockedIdsAsync(currentUserId);

            var query = _context.Follows
                .AsNoTracking()
                .Where(x => x.FollowerId == currentUserId && !blockedIds.Contains(x.FollowingId))
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Select(x => new FollowUserDto
                {
                    Id = x.Following!.Id,
                    Name = x.Following.Name,
                    Email = x.Following.Email,
                    ProfileImage = x.Following.ProfileImage,
                    FollowedAt = x.CreatedAt
                });

            return ApiResponse<PagedResultDto<FollowUserDto>>.Ok(
                await PaginationHelper.ToPagedResultAsync(query, pagination));
        }

        public async Task<ApiResponse<PagedResultDto<FollowUserDto>>> GetUserFollowersAsync(
            int currentUserId, int targetUserId, PaginationParamsDto pagination)
        {
            await _context.EnsureUserExistsAsync(targetUserId);

            if (await IsBlockedAsync(currentUserId, targetUserId))
                throw new KeyNotFoundException("User not found.");

            var blockedIds = await GetBlockedIdsAsync(currentUserId);

            var query = _context.Follows
                .AsNoTracking()
                .Where(x => x.FollowingId == targetUserId && !blockedIds.Contains(x.FollowerId))
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Select(x => new FollowUserDto
                {
                    Id = x.Follower!.Id,
                    Name = x.Follower.Name,
                    Email = x.Follower.Email,
                    ProfileImage = x.Follower.ProfileImage,
                    FollowedAt = x.CreatedAt
                });

            return ApiResponse<PagedResultDto<FollowUserDto>>.Ok(
                await PaginationHelper.ToPagedResultAsync(query, pagination));
        }

        public async Task<ApiResponse<PagedResultDto<FollowUserDto>>> GetUserFollowingAsync(
            int currentUserId, int targetUserId, PaginationParamsDto pagination)
        {
            await _context.EnsureUserExistsAsync(targetUserId);

            if (await IsBlockedAsync(currentUserId, targetUserId))
                throw new KeyNotFoundException("User not found.");

            var blockedIds = await GetBlockedIdsAsync(currentUserId);

            var query = _context.Follows
                .AsNoTracking()
                .Where(x => x.FollowerId == targetUserId && !blockedIds.Contains(x.FollowingId))
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Select(x => new FollowUserDto
                {
                    Id = x.Following!.Id,
                    Name = x.Following.Name,
                    Email = x.Following.Email,
                    ProfileImage = x.Following.ProfileImage,
                    FollowedAt = x.CreatedAt
                });

            return ApiResponse<PagedResultDto<FollowUserDto>>.Ok(
                await PaginationHelper.ToPagedResultAsync(query, pagination));
        }

        public async Task<ApiResponse<FollowStatusDto>> GetFollowStatusAsync(int currentUserId, int targetUserId)
        {
            var isFollowing = await _context.Follows.AnyAsync(x => x.FollowerId == currentUserId && x.FollowingId == targetUserId);
            var isFollowedBy = await _context.Follows.AnyAsync(x => x.FollowerId == targetUserId && x.FollowingId == currentUserId);

            return ApiResponse<FollowStatusDto>.Ok(new FollowStatusDto
            {
                IsFollowing = isFollowing,
                IsFollowedBy = isFollowedBy,
                IsMutual = isFollowing && isFollowedBy
            });
        }

        public async Task<ApiResponse<FollowCountsDto>> GetFollowCountsAsync(int currentUserId, int targetUserId)
        {
            await _context.EnsureUserExistsAsync(targetUserId);

            var followers = await _context.Follows.CountAsync(x => x.FollowingId == targetUserId);
            var following = await _context.Follows.CountAsync(x => x.FollowerId == targetUserId);

            return ApiResponse<FollowCountsDto>.Ok(new FollowCountsDto
            {
                Followers = followers,
                Following = following
            });
        }

        public async Task<ApiResponse<List<SuggestedUserDto>>> GetSuggestionsAsync(int currentUserId)
        {
            var blockedIds = await GetBlockedIdsAsync(currentUserId);
            var followingIds = await GetFollowingIdsAsync(currentUserId);

            var suggestions = await _context.Users
                .AsNoTracking()
                .Where(x =>
                    x.Id != currentUserId &&
                    !followingIds.Contains(x.Id) &&
                    !blockedIds.Contains(x.Id))
                .OrderByDescending(x => x.Id)
                .Take(10)
                .Select(x => new SuggestedUserDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    ProfileImage = x.ProfileImage,
                    MutualCount = 0
                })
                .ToListAsync();

            return ApiResponse<List<SuggestedUserDto>>.Ok(suggestions);
        }

        public async Task<ApiResponse<List<SuggestedUserDto>>> GetCommonSuggestionsAsync(int currentUserId)
        {
            var blockedIds = await GetBlockedIdsAsync(currentUserId);
            var followingIds = await GetFollowingIdsAsync(currentUserId);

            var candidates = await _context.Follows
                .Where(x => followingIds.Contains(x.FollowerId))
                .Select(x => x.FollowingId)
                .Where(x => x != currentUserId && !followingIds.Contains(x) && !blockedIds.Contains(x))
                .GroupBy(x => x)
                .Select(g => new { UserId = g.Key, MutualCount = g.Count() })
                .OrderByDescending(x => x.MutualCount)
                .Take(10)
                .ToListAsync();

            if (!candidates.Any())
                return ApiResponse<List<SuggestedUserDto>>.Ok(new List<SuggestedUserDto>());

            var ids = candidates.Select(x => x.UserId).ToList();
            var mutualLookup = candidates.ToDictionary(x => x.UserId, x => x.MutualCount);

            var users = await _context.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new SuggestedUserDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    ProfileImage = x.ProfileImage,
                    MutualCount = mutualLookup.ContainsKey(x.Id) ? mutualLookup[x.Id] : 0
                })
                .ToListAsync();

            return ApiResponse<List<SuggestedUserDto>>.Ok(
                users.OrderByDescending(x => x.MutualCount).ToList());
        }

        public async Task<ApiResponse<PagedResultDto<SuggestedUserDto>>> SearchUsersAsync(
            int currentUserId, string query, PaginationParamsDto pagination)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query is required.");

            var blockedIds = await GetBlockedIdsAsync(currentUserId);

            var dbQuery = _context.Users
                .AsNoTracking()
                .Where(x =>
                    x.Id != currentUserId &&
                    !blockedIds.Contains(x.Id) &&
                    (x.Name.Contains(query) || x.Email.Contains(query)))
                .OrderBy(x => x.Name)
                .Select(x => new SuggestedUserDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    ProfileImage = x.ProfileImage,
                    MutualCount = 0
                });

            return ApiResponse<PagedResultDto<SuggestedUserDto>>.Ok(
                await PaginationHelper.ToPagedResultAsync(dbQuery, pagination));
        }

        public async Task<ApiResponse<List<MutualUserDto>>> GetMutualFollowingAsync(
            int currentUserId, int targetUserId)
        {
            if (await IsBlockedAsync(currentUserId, targetUserId))
                throw new KeyNotFoundException("User not found.");

            var blockedIds = await GetBlockedIdsAsync(currentUserId);
            var myFollowingIds = await GetFollowingIdsAsync(currentUserId);
            var targetFollowingIds = await GetFollowingIdsAsync(targetUserId);

            var mutualIds = myFollowingIds
                .Intersect(targetFollowingIds)
                .Where(x => !blockedIds.Contains(x))
                .ToList();

            if (!mutualIds.Any())
                return ApiResponse<List<MutualUserDto>>.Ok(new List<MutualUserDto>());

            var users = await _context.Users
                .AsNoTracking()
                .Where(x => mutualIds.Contains(x.Id))
                .Select(x => new MutualUserDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProfileImage = x.ProfileImage
                })
                .ToListAsync();

            return ApiResponse<List<MutualUserDto>>.Ok(users);
        }

        public async Task<ApiResponse<string>> BlockUserAsync(int currentUserId, int targetUserId)
        {
            if (currentUserId == targetUserId)
                throw new ArgumentException("You cannot block yourself.");

            await _context.EnsureUserExistsAsync(targetUserId);

            if (await _context.BlockedUsers.AnyAsync(x => x.BlockerId == currentUserId && x.BlockedUserId == targetUserId))
                throw new ArgumentException("User is already blocked.");

            var follows = await _context.Follows
                .Where(x =>
                    (x.FollowerId == currentUserId && x.FollowingId == targetUserId) ||
                    (x.FollowerId == targetUserId && x.FollowingId == currentUserId))
                .ToListAsync();

            if (follows.Any())
                _context.Follows.RemoveRange(follows);

            _context.BlockedUsers.Add(new BlockedUser
            {
                BlockerId = currentUserId,
                BlockedUserId = targetUserId
            });

            await _context.SaveChangesAsync();
            _logger.LogInformation("User {A} blocked User {B}", currentUserId, targetUserId);
            return ApiResponse<string>.Ok("User blocked successfully.");
        }

        public async Task<ApiResponse<string>> UnblockUserAsync(int currentUserId, int targetUserId)
        {
            var block = await _context.BlockedUsers
                .FirstOrDefaultAsync(x => x.BlockerId == currentUserId && x.BlockedUserId == targetUserId);

            if (block == null)
                throw new ArgumentException("This user is not blocked.");

            _context.BlockedUsers.Remove(block);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {A} unblocked User {B}", currentUserId, targetUserId);
            return ApiResponse<string>.Ok("User unblocked successfully.");
        }

        public async Task<ApiResponse<PagedResultDto<BlockedUserDto>>> GetBlockedUsersAsync(
            int currentUserId, PaginationParamsDto pagination)
        {
            var query = _context.BlockedUsers
                .AsNoTracking()
                .Where(x => x.BlockerId == currentUserId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new BlockedUserDto
                {
                    Id = x.BlockedUserInfo!.Id,
                    Name = x.BlockedUserInfo.Name,
                    Email = x.BlockedUserInfo.Email,
                    ProfileImage = x.BlockedUserInfo.ProfileImage,
                    BlockedAt = x.CreatedAt
                });

            return ApiResponse<PagedResultDto<BlockedUserDto>>.Ok(
                await PaginationHelper.ToPagedResultAsync(query, pagination));
        }

        public async Task<ApiResponse<BlockStatusDto>> GetBlockStatusAsync(int currentUserId, int targetUserId)
        {
            var iBlockedThem = await _context.BlockedUsers.AnyAsync(x => x.BlockerId == currentUserId && x.BlockedUserId == targetUserId);
            var theyBlockedMe = await _context.BlockedUsers.AnyAsync(x => x.BlockerId == targetUserId && x.BlockedUserId == currentUserId);

            return ApiResponse<BlockStatusDto>.Ok(new BlockStatusDto
            {
                IBlockedThem = iBlockedThem,
                TheyBlockedMe = theyBlockedMe
            });
        }
    }
}