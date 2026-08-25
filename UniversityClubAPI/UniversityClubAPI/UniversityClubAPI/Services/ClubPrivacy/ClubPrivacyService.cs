using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.ClubPrivacy;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Services.ClubPrivacyService
{
    public class ClubPrivacyService : IClubPrivacyService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClubPrivacyService> _logger;
        private readonly INotificationService _notificationService;

        public ClubPrivacyService(AppDbContext context, ILogger<ClubPrivacyService> logger, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
            => ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);

        private static InviteResponseDto ToDto(ClubInvite i) => new()
        {
            Id = i.Id,
            ClubId = i.ClubId,
            ClubName = i.Club?.Name,
            InvitedUserId = i.InvitedUserId,
            InvitedUserName = i.InvitedUser?.Name,
            InvitedBy = i.InvitedBy,
            InviterName = i.Inviter?.Name,
            Status = i.Status,
            CreatedAt = i.CreatedAt,
            RespondedAt = i.RespondedAt
        };

        public async Task<ApiResponse<string>> UpdateVisibilityAsync(int currentUserId, int clubId, UpdateVisibilityDto dto)
        {
            var club = await _context.Clubs.FirstOrDefaultAsync(x => x.Id == clubId);
            if (club == null)
                return ApiResponse<string>.Fail("Club not found.");

            var member = await _context.GetMembershipAsync(currentUserId, clubId);
            if (member == null || !ClubPermissionHelper.IsAdmin(member.Role))
                return ApiResponse<string>.Fail("Only the Club Admin can change visibility settings.");

            if (club.Visibility == dto.Visibility)
                return ApiResponse<string>.Ok($"Club visibility is already {dto.Visibility}.");

            club.Visibility = dto.Visibility;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Club {ClubId} visibility changed to {Visibility} by {UserId}",
                clubId, dto.Visibility, currentUserId);

            return ApiResponse<string>.Ok($"Club visibility updated to {dto.Visibility}.");
        }

        public async Task<ApiResponse<InviteResponseDto>> CreateInviteAsync(int currentUserId, int clubId, CreateInviteDto dto)
        {
            var club = await _context.Clubs.FirstOrDefaultAsync(x => x.Id == clubId);
            if (club == null)
                return ApiResponse<InviteResponseDto>.Fail("Club not found.");

            var member = await _context.GetMembershipAsync(currentUserId, clubId);
            if (member == null || !ClubPermissionHelper.CanManage(member.Role))
                return ApiResponse<InviteResponseDto>.Fail("Only Admins or Moderators can invite users.");

            if (dto.InvitedUserId == currentUserId)
                return ApiResponse<InviteResponseDto>.Fail("You cannot invite yourself.");

            var targetUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == dto.InvitedUserId);
            if (targetUser == null)
                return ApiResponse<InviteResponseDto>.Fail("User not found.");

            if (await _context.IsBlockedEitherWayAsync(currentUserId, dto.InvitedUserId))
                return ApiResponse<InviteResponseDto>.Fail("Unable to invite this user.");

            var alreadyMember = await _context.GetMembershipAsync(dto.InvitedUserId, clubId);
            if (alreadyMember != null)
                return ApiResponse<InviteResponseDto>.Fail("This user is already a member of the club.");

            var existingPending = await _context.ClubInvites.AnyAsync(x =>
                x.ClubId == clubId && x.InvitedUserId == dto.InvitedUserId && x.Status == InviteStatus.Pending);

            if (existingPending)
                return ApiResponse<InviteResponseDto>.Fail("This user already has a pending invite to this club.");

            var invite = new ClubInvite
            {
                ClubId = clubId,
                InvitedUserId = dto.InvitedUserId,
                InvitedBy = currentUserId
            };

            _context.ClubInvites.Add(invite);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                return ApiResponse<InviteResponseDto>.Fail("This user already has a pending invite to this club.");
            }

            await _notificationService.CreateAndPushAsync(new DTOs.Notification.CreateNotificationDto
            {
                SenderId = currentUserId,
                ReceiverId = dto.InvitedUserId,
                Type = NotificationType.ClubInvite,
                Message = $"You've been invited to join {club.Name}"
            });

            _logger.LogInformation("User {InvitedUserId} invited to Club {ClubId} by {InviterId}",
                dto.InvitedUserId, clubId, currentUserId);

            var result = await _context.ClubInvites
                .Include(x => x.Club)
                .Include(x => x.InvitedUser)
                .Include(x => x.Inviter)
                .FirstAsync(x => x.Id == invite.Id);

            return ApiResponse<InviteResponseDto>.Ok(ToDto(result), "Invite sent successfully.");
        }

        public async Task<ApiResponse<string>> RevokeInviteAsync(int currentUserId, int inviteId)
        {
            var invite = await _context.ClubInvites.FirstOrDefaultAsync(x => x.Id == inviteId);
            if (invite == null)
                return ApiResponse<string>.Fail("Invite not found.");

            var member = await _context.GetMembershipAsync(currentUserId, invite.ClubId);
            if (member == null || !ClubPermissionHelper.CanManage(member.Role))
                return ApiResponse<string>.Fail("Only Admins or Moderators can revoke invites.");

            if (invite.Status != InviteStatus.Pending)
                return ApiResponse<string>.Fail("Only pending invites can be revoked.");

            invite.Status = InviteStatus.Revoked;
            invite.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse<string>.Ok("Invite revoked.");
        }

        public async Task<ApiResponse<PagedResultDto<InviteResponseDto>>> GetClubInvitesAsync(
            int currentUserId, int clubId, PaginationParamsDto pagination, InviteStatus? status)
        {
            var member = await _context.GetMembershipAsync(currentUserId, clubId);
            if (member == null || !ClubPermissionHelper.CanManage(member.Role))
                return ApiResponse<PagedResultDto<InviteResponseDto>>.Fail("Only Admins or Moderators can view invites.");

            var query = _context.ClubInvites
                .AsNoTracking()
                .Include(x => x.Club)
                .Include(x => x.InvitedUser)
                .Include(x => x.Inviter)
                .Where(x => x.ClubId == clubId)
                .Where(x => status == null || x.Status == status)
                .OrderByDescending(x => x.CreatedAt);

            var paged = await PaginationHelper.ToPagedResultAsync(query, pagination);

            return ApiResponse<PagedResultDto<InviteResponseDto>>.Ok(new PagedResultDto<InviteResponseDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(ToDto).ToList()
            });
        }

        public async Task<ApiResponse<InviteResponseDto>> GetInviteByIdAsync(int currentUserId, int inviteId)
        {
            var invite = await _context.ClubInvites
                .AsNoTracking()
                .Include(x => x.Club)
                .Include(x => x.InvitedUser)
                .Include(x => x.Inviter)
                .FirstOrDefaultAsync(x => x.Id == inviteId);

            if (invite == null)
                return ApiResponse<InviteResponseDto>.Fail("Invite not found.");

            if (invite.InvitedUserId == currentUserId)
                return ApiResponse<InviteResponseDto>.Ok(ToDto(invite));

            var member = await _context.GetMembershipAsync(currentUserId, invite.ClubId);
            if (member != null && ClubPermissionHelper.CanManage(member.Role))
                return ApiResponse<InviteResponseDto>.Ok(ToDto(invite));

            return ApiResponse<InviteResponseDto>.Fail("You don't have permission to view this invite.");
        }

        public async Task<ApiResponse<List<InviteResponseDto>>> GetMyInvitesAsync(int userId)
        {
            var invites = await _context.ClubInvites
                .AsNoTracking()
                .Include(x => x.Club)
                .Include(x => x.InvitedUser)
                .Include(x => x.Inviter)
                .Where(x => x.InvitedUserId == userId && x.Status == InviteStatus.Pending)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return ApiResponse<List<InviteResponseDto>>.Ok(invites.Select(ToDto).ToList());
        }

        public async Task<ApiResponse<string>> AcceptInviteAsync(int userId, int inviteId)
        {
            var invite = await _context.ClubInvites
                .Include(x => x.Club)
                .FirstOrDefaultAsync(x => x.Id == inviteId);

            if (invite == null)
                return ApiResponse<string>.Fail("Invite not found.");

            if (invite.InvitedUserId != userId)
                return ApiResponse<string>.Fail("This invite does not belong to you.");

            if (invite.Status != InviteStatus.Pending)
                return ApiResponse<string>.Fail("This invite is no longer pending.");

            var alreadyMember = await _context.GetMembershipAsync(userId, invite.ClubId);
            if (alreadyMember != null)
            {
                invite.Status = InviteStatus.Accepted;
                invite.RespondedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return ApiResponse<string>.Ok("You are already a member of this club.");
            }

            invite.Status = InviteStatus.Accepted;
            invite.RespondedAt = DateTime.UtcNow;

            _context.ClubMembers.Add(new ClubMember
            {
                ClubId = invite.ClubId,
                UserId = userId,
                Role = "Member",
                IsApproved = true
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {

                _logger.LogWarning("Duplicate membership insert avoided for User {UserId} in Club {ClubId}",
                    userId, invite.ClubId);
                return ApiResponse<string>.Ok("You are already a member of this club.");
            }

            await _notificationService.CreateAndPushAsync(new DTOs.Notification.CreateNotificationDto
            {
                SenderId = userId,
                ReceiverId = invite.InvitedBy,
                Type = NotificationType.ClubInvite,
                Message = $"Your invite to {invite.Club?.Name} was accepted."
            });

            _logger.LogInformation("User {UserId} accepted invite to Club {ClubId}", userId, invite.ClubId);

            return ApiResponse<string>.Ok($"You've joined {invite.Club?.Name}!");
        }

        public async Task<ApiResponse<string>> DeclineInviteAsync(int userId, int inviteId)
        {
            var invite = await _context.ClubInvites
                .Include(x => x.Club)
                .FirstOrDefaultAsync(x => x.Id == inviteId);

            if (invite == null)
                return ApiResponse<string>.Fail("Invite not found.");

            if (invite.InvitedUserId != userId)
                return ApiResponse<string>.Fail("This invite does not belong to you.");

            if (invite.Status != InviteStatus.Pending)
                return ApiResponse<string>.Fail("This invite is no longer pending.");

            invite.Status = InviteStatus.Declined;
            invite.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _notificationService.CreateAndPushAsync(new DTOs.Notification.CreateNotificationDto
            {
                SenderId = userId,
                ReceiverId = invite.InvitedBy,
                Type = NotificationType.ClubInvite,
                Message = $"Your invite to {invite.Club?.Name} was declined."
            });

            return ApiResponse<string>.Ok("Invite declined.");
        }
    }
}