using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.LiveEvent;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Hubs;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.NotificationService;


namespace UniversityClubAPI.Services.LiveEventService
{
    public class LiveEventService : ILiveEventService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<LiveEventHub> _hub;
        private readonly ILogger<LiveEventService> _logger;
        private readonly INotificationService _notificationService;

        public LiveEventService(AppDbContext context, IHubContext<LiveEventHub> hub, ILogger<LiveEventService> logger, INotificationService notificationService)
        {
            _context = context;
            _hub = hub;
            _logger = logger;
            _notificationService = notificationService;
        }

        private async Task<ClubMember?> GetMembershipAsync(int userId, int clubId)
            => await _context.ClubMembers.FirstOrDefaultAsync(x => x.UserId == userId && x.ClubId == clubId);

        private static string RoomGroup(int eventId) => $"live-{eventId}";

        private async Task<int> GetViewerCountAsync(int eventId)
            => await _context.LiveParticipants.CountAsync(x => x.EventId == eventId && x.LeftAt == null);

        public async Task<ApiResponse<LiveSessionResponseDto>> StartLiveAsync(int userId, int eventId, StartLiveDto dto)
        {
            var ev = await _context.Events.Include(x => x.club).FirstOrDefaultAsync(x => x.Id == eventId);
            if (ev == null)
                throw new KeyNotFoundException("Event not found.");

            var member = await _context.GetMembershipAsync(userId, ev.ClubId);
            if (member == null || !ClubPermissionHelper.CanManage(member.Role))
                throw new UnauthorizedAccessException("Only Admins or Moderators can start a live session.");

            if (ev.LiveStatus == LiveStatus.Live)
                throw new ArgumentException("This event is already live.");

            if (ev.LiveStatus == LiveStatus.Ended)
                throw new ArgumentException("This live session has already ended.");

            ev.LiveStatus = LiveStatus.Live;
            ev.MeetingLink = dto.MeetingLink;
            ev.LiveStartedAt = DateTime.UtcNow;

            var attendeeIds = await _context.EventAttendances
                .Where(x => x.EventId == eventId && x.UserId != userId)
                .Select(x => x.UserId)
                .ToListAsync();

            foreach (var attendeeId in attendeeIds)
            {
                await _notificationService.CreateAndPushAsync(new CreateNotificationDto
                {
                    SenderId = userId,
                    ReceiverId = attendeeId,
                    Type = NotificationType.EventLive,
                    Message = $"{ev.Title} is live now!"
                });
            }

            await _context.SaveChangesAsync();

            await _hub.Clients.Group(RoomGroup(eventId)).SendAsync("LiveStarted", new
            {
                eventId,
                meetingLink = ev.MeetingLink,
                startedAt = ev.LiveStartedAt
            });

            _logger.LogInformation("Event {EventId} went live, started by {UserId}", eventId, userId);

            return ApiResponse<LiveSessionResponseDto>.Ok(await BuildStatusDto(ev, userId, member), "Live session started.");
        }

        public async Task<ApiResponse<LiveSessionResponseDto>> EndLiveAsync(int userId, int eventId)
        {
            var ev = await _context.Events.Include(x => x.club).FirstOrDefaultAsync(x => x.Id == eventId);
            if (ev == null)
                throw new KeyNotFoundException("Event not found.");

            var member = await _context.GetMembershipAsync(userId, ev.ClubId);
            if (member == null || !ClubPermissionHelper.CanManage(member.Role))
                throw new UnauthorizedAccessException("Only Admins or Moderators can end the live session.");

            if (ev.LiveStatus != LiveStatus.Live)
                throw new ArgumentException("This event is not currently live.");

            ev.LiveStatus = LiveStatus.Ended;
            ev.LiveEndedAt = DateTime.UtcNow;

            var openParticipants = await _context.LiveParticipants
                .Where(x => x.EventId == eventId && x.LeftAt == null)
                .ToListAsync();

            foreach (var p in openParticipants)
                p.LeftAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _hub.Clients.Group(RoomGroup(eventId)).SendAsync("LiveEnded", new
            {
                eventId,
                endedAt = ev.LiveEndedAt
            });

            _logger.LogInformation("Event {EventId} live session ended by {UserId}", eventId, userId);

            return ApiResponse<LiveSessionResponseDto>.Ok(await BuildStatusDto(ev, userId, member), "Live session ended.");
        }

        public async Task<ApiResponse<LiveSessionResponseDto>> GetStatusAsync(int userId, int eventId)
        {
            var ev = await _context.Events.Include(x => x.club).FirstOrDefaultAsync(x => x.Id == eventId);
            if (ev == null)
                throw new KeyNotFoundException("Event not found.");

            var member = await _context.GetMembershipAsync(userId, ev.ClubId);
            return ApiResponse<LiveSessionResponseDto>.Ok(await BuildStatusDto(ev, userId, member));
        }

        private async Task<LiveSessionResponseDto> BuildStatusDto(Event ev, int userId, ClubMember? member)
        {
            var canSeeLink = member != null && ev.LiveStatus != LiveStatus.NotStarted;

            return new LiveSessionResponseDto
            {
                EventId = ev.Id,
                Title = ev.Title,
                ClubId = ev.ClubId,
                ClubName = ev.club?.Name,
                Status = ev.LiveStatus,
                MeetingLink = canSeeLink ? ev.MeetingLink : null,
                LiveStartedAt = ev.LiveStartedAt,
                LiveEndedAt = ev.LiveEndedAt,
                CurrentViewerCount = ev.LiveStatus == LiveStatus.Live ? await GetViewerCountAsync(ev.Id) : 0
            };
        }

        public async Task<ApiResponse<PagedResultDto<LiveChatMessageDto>>> GetChatHistoryAsync(
            int userId, int eventId, PaginationParamsDto pagination)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (ev == null)
                throw new KeyNotFoundException("Event not found.");

            var member = await _context.GetMembershipAsync(userId, ev.ClubId);
            if (member == null)
                throw new UnauthorizedAccessException("Only club members can view the live chat.");

            var query = _context.LiveChatMessages
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.EventId == eventId)
                .OrderByDescending(x => x.SentAt);

            var paged = await PaginationHelper.ToPagedResultAsync(query, pagination);

            return ApiResponse<PagedResultDto<LiveChatMessageDto>>.Ok(new PagedResultDto<LiveChatMessageDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(m => new LiveChatMessageDto
                {
                    Id = m.Id,
                    EventId = m.EventId,
                    UserId = m.UserId,
                    UserName = m.User?.Name,
                    UserProfileImage = m.User?.ProfileImage,
                    Message = m.Message,
                    SentAt = m.SentAt
                }).ToList()
            });
        }

        public async Task<ApiResponse<List<LiveViewerDto>>> GetActiveViewersAsync(int userId, int eventId)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (ev == null)
                throw new KeyNotFoundException("Event not found.");

            var member = await _context.GetMembershipAsync(userId, ev.ClubId);
            if (member == null)
                throw new UnauthorizedAccessException("Only club members can view the active viewers list.");

            var viewers = await _context.LiveParticipants
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.EventId == eventId && x.LeftAt == null)
                .OrderBy(x => x.JoinedAt)
                .Select(x => new
                {
                    x.UserId,
                    UserName = x.User != null ? x.User.Name : null,
                    UserProfileImage = x.User != null ? x.User.ProfileImage : null,
                    x.JoinedAt
                })
                .ToListAsync();

            if (viewers.Count == 0)
                return ApiResponse<List<LiveViewerDto>>.Ok(new List<LiveViewerDto>());

            var viewerIds = viewers.Select(v => v.UserId).ToList();

            var moderationByUser = await _context.LiveModerations
                .AsNoTracking()
                .Where(x => x.EventId == eventId && viewerIds.Contains(x.UserId))
                .ToDictionaryAsync(x => x.UserId, x => x);

            var result = viewers.Select(v =>
            {
                moderationByUser.TryGetValue(v.UserId, out var mod);
                return new LiveViewerDto
                {
                    UserId = v.UserId,
                    UserName = v.UserName,
                    UserProfileImage = v.UserProfileImage,
                    JoinedAt = v.JoinedAt,
                    IsMuted = mod?.IsMuted == true,
                    IsBanned = mod?.IsBanned == true
                };
            }).ToList();

            return ApiResponse<List<LiveViewerDto>>.Ok(result);
        }

        private async Task<ClubMember> ValidateModeratorAsync(int moderatorId, Event ev)
        {
            var modMember = await _context.GetMembershipAsync(moderatorId, ev.ClubId);
            if (modMember == null || !ClubPermissionHelper.CanManage(modMember.Role))
                throw new UnauthorizedAccessException("Only Admins or Moderators can perform this action.");
            return modMember;
        }

        private static bool CanModerateTarget(ClubMember modMember, ClubMember? targetMember)
        {
            if (targetMember == null) return true;

            var modIsAdmin = ClubPermissionHelper.IsAdmin(modMember.Role);
            var targetHasElevatedRole = ClubPermissionHelper.CanManage(targetMember.Role);

            if (targetHasElevatedRole && !modIsAdmin) return false;

            return true;
        }

        private async Task<LiveModeration> GetOrCreateModerationAsync(int eventId, int targetUserId, int moderatorId)
        {
            var row = await _context.LiveModerations
                .FirstOrDefaultAsync(x => x.EventId == eventId && x.UserId == targetUserId);

            if (row == null)
            {
                row = new LiveModeration
                {
                    EventId = eventId,
                    UserId = targetUserId,
                    ModeratedBy = moderatorId
                };
                _context.LiveModerations.Add(row);
            }

            return row;
        }

        public async Task<ApiResponse<LiveModerationStatusDto>> MuteUserAsync(int moderatorId, int eventId, int targetUserId, MuteRequestDto dto)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (ev == null)
                throw new KeyNotFoundException("Event not found.");

            var modMember = await ValidateModeratorAsync(moderatorId, ev);

            if (targetUserId == moderatorId)
                throw new ArgumentException("You cannot mute yourself.");

            var targetMember = await _context.GetMembershipAsync(targetUserId, ev.ClubId);
            if (targetMember == null)
                throw new KeyNotFoundException("Target user is not a member of this club.");

            if (!CanModerateTarget(modMember, targetMember))
                throw new UnauthorizedAccessException("Moderators cannot mute Admins or other Moderators.");

            var row = await GetOrCreateModerationAsync(eventId, targetUserId, moderatorId);
            row.IsMuted = dto.Mute;
            row.ModeratedBy = moderatorId;
            row.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var targetUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == targetUserId);

            await _hub.Clients.User(targetUserId.ToString())
                .SendAsync("MuteStatusChanged", new { eventId, isMuted = row.IsMuted });

            _logger.LogInformation("User {TargetUserId} {Action} in event {EventId} by {ModeratorId}",
                targetUserId, dto.Mute ? "muted" : "unmuted", eventId, moderatorId);

            return ApiResponse<LiveModerationStatusDto>.Ok(new LiveModerationStatusDto
            {
                UserId = targetUserId,
                UserName = targetUser?.Name,
                IsMuted = row.IsMuted,
                IsBanned = row.IsBanned
            }, dto.Mute ? "User muted." : "User unmuted.");
        }

        public async Task<ApiResponse<LiveModerationStatusDto>> KickUserAsync(int moderatorId, int eventId, int targetUserId, KickRequestDto dto)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (ev == null)
                throw new KeyNotFoundException("Event not found.");

            var modMember = await ValidateModeratorAsync(moderatorId, ev);

            if (targetUserId == moderatorId)
                throw new ArgumentException("You cannot kick yourself.");

            var targetMember = await _context.GetMembershipAsync(targetUserId, ev.ClubId);
            if (targetMember == null)
                throw new KeyNotFoundException("Target user is not a member of this club.");

            if (!CanModerateTarget(modMember, targetMember))
                throw new UnauthorizedAccessException("Moderators cannot kick Admins or other Moderators.");

            var row = await GetOrCreateModerationAsync(eventId, targetUserId, moderatorId);
            if (dto.Ban)
                row.IsBanned = true;
            row.ModeratedBy = moderatorId;
            row.UpdatedAt = DateTime.UtcNow;

            var openEntry = await _context.LiveParticipants
                .Where(x => x.EventId == eventId && x.UserId == targetUserId && x.LeftAt == null)
                .OrderByDescending(x => x.JoinedAt)
                .FirstOrDefaultAsync();

            if (openEntry != null)
                openEntry.LeftAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var targetUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == targetUserId);

            await _hub.Clients.User(targetUserId.ToString())
                .SendAsync("KickedFromLive", new { eventId, banned = row.IsBanned });

            var viewerCount = await GetViewerCountAsync(eventId);
            await _hub.Clients.Group(RoomGroup(eventId))
                .SendAsync("ViewerCountUpdated", new { eventId, viewerCount });

            _logger.LogInformation("User {TargetUserId} kicked (banned={Banned}) from event {EventId} by {ModeratorId}",
                targetUserId, row.IsBanned, eventId, moderatorId);

            return ApiResponse<LiveModerationStatusDto>.Ok(new LiveModerationStatusDto
            {
                UserId = targetUserId,
                UserName = targetUser?.Name,
                IsMuted = row.IsMuted,
                IsBanned = row.IsBanned
            }, row.IsBanned ? "User kicked and banned from this live session." : "User kicked.");
        }

        public async Task<ApiResponse<string>> UnbanUserAsync(int moderatorId, int eventId, int targetUserId)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (ev == null)
                throw new KeyNotFoundException("Event not found.");

            await ValidateModeratorAsync(moderatorId, ev);

            var row = await _context.LiveModerations
                .FirstOrDefaultAsync(x => x.EventId == eventId && x.UserId == targetUserId);

            if (row == null || !row.IsBanned)
                throw new ArgumentException("This user is not currently banned from this event.");

            row.IsBanned = false;
            row.ModeratedBy = moderatorId;
            row.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("User unbanned.", "User can now rejoin the live session.");
        }
    }
}