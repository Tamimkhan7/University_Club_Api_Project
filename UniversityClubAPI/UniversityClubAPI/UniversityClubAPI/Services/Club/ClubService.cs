using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs;
using UniversityClubAPI.DTOs.Club;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Services.ClubService
{
    public class ClubService : IClubService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClubService> _logger;

        public ClubService(AppDbContext context, ILogger<ClubService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task EnsureCanViewClubContentAsync(int userId, int clubId)
        {
            var clubExists = await _context.Clubs.AnyAsync(x => x.Id == clubId);
            if (!clubExists)
                throw new KeyNotFoundException("Club not found");

            var isApprovedMember = await _context.IsApprovedMemberAsync(userId, clubId);

            if (!isApprovedMember)
                throw new UnauthorizedAccessException("Join this club to view its content.");
        }

        public async Task<ApiResponse<object>> CreateClubAsync(int userId, CreateClubDTO dto)
        {
            var user = await _context.GetUserOrThrowAsync(userId);

            var exists = await _context.Clubs.AnyAsync(x => x.Name == dto.Name);
            if (exists)
                throw new ArgumentException("Club already exists");

            var club = new Club
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedBy = userId
            };

            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();

            _context.ClubMembers.Add(new ClubMember
            {
                ClubId = club.Id,
                UserId = userId,
                Role = "Admin",
                IsApproved = true
            });

            await _context.SaveChangesAsync();

            return ApiResponse<object>.Ok(club, "Club created successfully");
        }

        public async Task<ApiResponse<object>> JoinClubAsync(int userId, JoinClubDTO dto)
        {
            var club = await _context.GetClubOrThrowAsync(dto.ClubId);

            var exists = await _context.IsMemberAsync(userId, dto.ClubId);

            if (exists)
                throw new ArgumentException("Already joined");

            var member = new ClubMember
            {
                UserId = userId,
                ClubId = dto.ClubId,
                Role = "Member",
                IsApproved = true
            };

            _context.ClubMembers.Add(member);
            await _context.SaveChangesAsync();

            return ApiResponse<object>.Ok(member, "Joined successfully");
        }

        public async Task<ApiResponse<string>> LeaveClubAsync(int userId, int clubId)
        {
            var member = await _context.GetMembershipAsync(userId, clubId);

            if (member == null)
                throw new KeyNotFoundException("not a member");

            _context.ClubMembers.Remove(member);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Left club successfully");
        }

        public async Task<ApiResponse<object>> GetAllClubsAsync(PaginationParamsDto pagination)
        {
            var query = _context.Clubs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.CreatedBy,
                    x.CreatedAt,
                    x.Visibility,
                    MemberCount = _context.ClubMembers.Count(c => c.ClubId == x.Id)
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, pagination);
            return ApiResponse<object>.Ok(result);
        }

        public async Task<ApiResponse<object>> GetClubByIdAsync(int userId, int id)
        {
            await EnsureCanViewClubContentAsync(userId, id);

            var club = await _context.Clubs
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.CreatedBy,
                    x.CreatedAt,
                    x.Visibility,
                    MemberCount = _context.ClubMembers.Count(c => c.ClubId == x.Id)
                })
                .FirstOrDefaultAsync();

            if (club == null)
                throw new KeyNotFoundException("Club not found");

            return ApiResponse<object>.Ok(club);
        }

        public async Task<ApiResponse<object>> UpdateClubAsync(int userId, int id, CreateClubDTO dto)
        {
            var club = await _context.GetClubOrThrowAsync(id);

            if (club.CreatedBy != userId)
                throw new UnauthorizedAccessException("Only the club creator can update this club");

            var exists = await _context.Clubs
                .AnyAsync(x => x.Name == dto.Name && x.Id != id);

            if (exists)
                throw new ArgumentException("Name already exists");

            club.Name = dto.Name;
            club.Description = dto.Description;

            await _context.SaveChangesAsync();
            return ApiResponse<object>.Ok(club, "Updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteClubAsync(int userId, int id)
        {
            var club = await _context.GetClubOrThrowAsync(id);

            if (club.CreatedBy != userId)
                throw new UnauthorizedAccessException("Only the club creator can delete this club");

            _context.Reactions.RemoveRange(_context.Reactions.Where(r => r.Post != null && r.Post.ClubId == id));
            _context.Comments.RemoveRange(_context.Comments.Where(c => c.Post != null && c.Post.ClubId == id));
            _context.Posts.RemoveRange(_context.Posts.Where(p => p.ClubId == id));

            var eventIds = await _context.Events
                .Where(e => e.ClubId == id)
                .Select(e => e.Id)
                .ToListAsync();

            if (eventIds.Count > 0)
            {
                _context.EventAttendances.RemoveRange(_context.EventAttendances.Where(a => eventIds.Contains(a.EventId)));
                _context.Events.RemoveRange(_context.Events.Where(e => e.ClubId == id));
            }

            _context.ClubInvites.RemoveRange(_context.ClubInvites.Where(i => i.ClubId == id));
            _context.ClubMembers.RemoveRange(_context.ClubMembers.Where(m => m.ClubId == id));

            _context.Clubs.Remove(club);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "Failed to delete Club {ClubId}; other data (e.g. polls, files, groups) may still reference it.", id);
                throw new InvalidOperationException(
                    "This club still has related data that must be removed before it can be deleted.");
            }

            return ApiResponse<string>.Ok("Deleted successfully");
        }

        public async Task<ApiResponse<object>> GetMembersAsync(int userId, int clubId, PaginationParamsDto pagination)
        {
            await EnsureCanViewClubContentAsync(userId, clubId);

            var query = _context.ClubMembers
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.ClubId == clubId)
                .OrderByDescending(x => x.JoinedAt)
                .Select(x => new
                {
                    x.UserId,
                    x.Role,
                    x.JoinedAt,
                    userName = x.User!.Name,
                    userImage = x.User.ProfileImage
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, pagination);
            return ApiResponse<object>.Ok(result);
        }

        public async Task<ApiResponse<string>> UpdateRoleAsync(int currentUserId, int clubId, UpdateClubRoleDto dto)
        {
            await _context.RequireAdminAsync(currentUserId, clubId, "Only admin allowed");

            var member = await _context.GetMembershipAsync(dto.UserId, clubId);

            if (member == null)
                throw new KeyNotFoundException("Member not found");

            var roles = new[] { "Admin", "Moderator", "Member" };
            if (!roles.Contains(dto.Role))
                throw new ArgumentException("Invalid role");

            member.Role = dto.Role;
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Role updated");
        }

        public async Task<ApiResponse<string>> RemoveMemberAsync(int currentUserId, int clubId, int userId)
        {
            await _context.RequireManagerAsync(currentUserId, clubId, "Unauthorized");

            var member = await _context.GetMembershipAsync(userId, clubId);

            if (member == null)
                throw new KeyNotFoundException("Member not found");

            _context.ClubMembers.Remove(member);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Removed");
        }

        public async Task<ApiResponse<object>> SearchClubsAsync(string query, PaginationParamsDto pagination)
        {
            var dbQuery = _context.Clubs
                .AsNoTracking()
                .Where(x =>
                    x.Name!.Contains(query) ||
                    (x.Description != null && x.Description.Contains(query)))
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.CreatedBy,
                    x.CreatedAt,
                    x.Visibility,
                    MemberCount = _context.ClubMembers.Count(c => c.ClubId == x.Id)
                });

            var result = await PaginationHelper.ToPagedResultAsync(dbQuery, pagination);
            return ApiResponse<object>.Ok(result);
        }

        public async Task<ApiResponse<object>> GetMyClubsAsync(int userId)
        {
            var clubs = await _context.ClubMembers
                .AsNoTracking()
                .Include(x => x.Club)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.JoinedAt)
                .Select(x => new
                {
                    x.ClubId,
                    ClubName = x.Club!.Name,
                    x.Role,
                    x.JoinedAt
                })
                .ToListAsync();

            return ApiResponse<object>.Ok(clubs);
        }

        public async Task<ApiResponse<object>> GetMembershipStatusAsync(int userId, int clubId)
        {
            var member = await _context.GetMembershipAsync(userId, clubId);

            var status = member == null
                ? new MemberStatusDto { IsMember = false, Role = null }
                : new MemberStatusDto { IsMember = true, Role = member.Role };

            return ApiResponse<object>.Ok(status);
        }

        public async Task<ApiResponse<object>> GetClubPostsAsync(int userId, int clubId, PaginationParamsDto pagination)
        {
            await EnsureCanViewClubContentAsync(userId, clubId);

            var query = _context.Posts
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.ClubId == clubId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.Content,
                    x.ImageUrl,
                    x.CreatedAt,
                    x.UserId,
                    UserName = x.User!.Name,
                    UserImage = x.User.ProfileImage,
                    CommentCount = x.Comments != null ? x.Comments.Count() : 0,
                    ReactionCount = x.Reactions != null ? x.Reactions.Count() : 0
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, pagination);
            return ApiResponse<object>.Ok(result);
        }

        public async Task<ApiResponse<object>> SearchMembersAsync(int userId, int clubId, string query, PaginationParamsDto pagination)
        {
            await EnsureCanViewClubContentAsync(userId, clubId);

            var keyword = query.ToLower().Trim();

            var dbQuery = _context.ClubMembers
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x =>
                    x.ClubId == clubId &&
                    x.User != null &&
                    x.User.Name.ToLower().Contains(keyword))
                .Select(x => new
                {
                    x.UserId,
                    UserName = x.User!.Name,
                    UserImage = x.User.ProfileImage,
                    x.Role,
                    x.JoinedAt
                });

            var result = await PaginationHelper.ToPagedResultAsync(dbQuery, pagination);
            return ApiResponse<object>.Ok(result);
        }
    }
}