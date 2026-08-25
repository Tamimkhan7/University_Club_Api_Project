using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Helpers
{
    public static class ClubMembershipHelper
    {
        public static async Task<ClubMember?> GetMembershipAsync(this AppDbContext context, int userId, int clubId)
            => await context.ClubMembers.FirstOrDefaultAsync(x => x.UserId == userId && x.ClubId == clubId);

        public static async Task<bool> IsMemberAsync(this AppDbContext context, int userId, int clubId)
            => await context.ClubMembers.AnyAsync(x => x.UserId == userId && x.ClubId == clubId);

        public static async Task<bool> IsApprovedMemberAsync(this AppDbContext context, int userId, int clubId)
            => await context.ClubMembers.AnyAsync(x => x.UserId == userId && x.ClubId == clubId && x.IsApproved);

        public static async Task<ClubMember> RequireManagerAsync(this AppDbContext context, int userId, int clubId, string errorMessage)
        {
            var member = await context.GetMembershipAsync(userId, clubId);
            if (member == null || !ClubPermissionHelper.CanManage(member.Role))
                throw new UnauthorizedAccessException(errorMessage);
            return member;
        }

        public static async Task<ClubMember> RequireAdminAsync(this AppDbContext context, int userId, int clubId, string errorMessage)
        {
            var member = await context.GetMembershipAsync(userId, clubId);
            if (member == null || !ClubPermissionHelper.IsAdmin(member.Role))
                throw new UnauthorizedAccessException(errorMessage);
            return member;
        }

        public static async Task<HashSet<int>> GetBlockedUserIdsAsync(this AppDbContext context, int userId)
        {
            var ids = await context.BlockedUsers
                .AsNoTracking()
                .Where(b => b.BlockerId == userId || b.BlockedUserId == userId)
                .Select(b => b.BlockerId == userId ? b.BlockedUserId : b.BlockerId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public static async Task<bool> IsBlockedEitherWayAsync(this AppDbContext context, int userIdA, int userIdB)
            => await context.BlockedUsers.AnyAsync(x =>
                (x.BlockerId == userIdA && x.BlockedUserId == userIdB) ||
                (x.BlockerId == userIdB && x.BlockedUserId == userIdA));

        public static async Task<Club> GetClubOrThrowAsync(this AppDbContext context, int clubId)
        {
            var club = await context.Clubs.FindAsync(clubId);
            if (club == null)
                throw new KeyNotFoundException("Club not found");

            return club;
        }

        public static async Task EnsureUserExistsAsync(this AppDbContext context, int userId)
        {
            var exists = await context.Users.AnyAsync(x => x.Id == userId);
            if (!exists)
                throw new KeyNotFoundException("User not found.");
        }

        public static async Task<User> GetUserOrThrowAsync(this AppDbContext context, int userId)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            return user;
        }
    }
}