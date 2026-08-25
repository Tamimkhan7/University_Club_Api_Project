using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Presence;
using UniversityClubAPI.Helpers;
namespace UniversityClubAPI.Services.PresenceService
{
    public class PresenceService : IPresenceService
    {
        private const int MaxBulkSize = 100;
        private readonly AppDbContext _context;
        public PresenceService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse<PresenceStatusDto>> GetStatusAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted);
            if (user == null)
                return ApiResponse<PresenceStatusDto>.Fail("User not found.");
            return ApiResponse<PresenceStatusDto>.Ok(new PresenceStatusDto
            {
                UserId = user.Id,
                UserName = user.Name,
                ProfileImage = user.ProfileImage,
                IsOnline = user.IsOnline,
                LastSeenAt = user.LastSeenAt
            });
        }
        public async Task<ApiResponse<List<PresenceStatusDto>>> GetBulkStatusAsync(List<int> userIds)
        {
            var distinctIds = (userIds ?? new List<int>()).Distinct().Take(MaxBulkSize).ToList();
            if (distinctIds.Count == 0)
                return ApiResponse<List<PresenceStatusDto>>.Ok(new List<PresenceStatusDto>());
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => distinctIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => new PresenceStatusDto
                {
                    UserId = u.Id,
                    UserName = u.Name,
                    ProfileImage = u.ProfileImage,
                    IsOnline = u.IsOnline,
                    LastSeenAt = u.LastSeenAt
                })
                .ToListAsync();
            return ApiResponse<List<PresenceStatusDto>>.Ok(users);
        }
        public async Task<ApiResponse<PagedResultDto<PresenceStatusDto>>> GetOnlineFollowingAsync(int currentUserId, PaginationParamsDto pagination)
        {
            var followingIds = await _context.Follows
                .Where(x => x.FollowerId == currentUserId)
                .Select(x => x.FollowingId)
                .ToListAsync();

            var q = _context.Users
                .AsNoTracking()
                .Where(u => followingIds.Contains(u.Id) && u.IsOnline && !u.IsDeleted)
                .OrderByDescending(u => u.LastSeenAt)
                .Select(u => new PresenceStatusDto
                {
                    UserId = u.Id,
                    UserName = u.Name,
                    ProfileImage = u.ProfileImage,
                    IsOnline = u.IsOnline,
                    LastSeenAt = u.LastSeenAt
                });

            var paged = await PaginationHelper.ToPagedResultAsync(q, pagination);
            return ApiResponse<PagedResultDto<PresenceStatusDto>>.Ok(paged);
        }
    }
}