using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs;
using UniversityClubAPI.DTOs.Club;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Event;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Services.EventService
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public EventService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }


        public async Task<ApiResponse<EventResponseDto>> CreateAsync(int userId, CreateEventDto dto)
        {
            if (dto.EventDate <= DateTime.UtcNow)
                return ApiResponse<EventResponseDto>.Fail("Event date must be in the future.");

            var clubExists = await _context.Clubs.AnyAsync(x => x.Id == dto.ClubId);
            if (!clubExists)
                return ApiResponse<EventResponseDto>.Fail("Club does not exist.");

            var member = await _context.GetMembershipAsync(userId, userId);
            if (member == null)
                return ApiResponse<EventResponseDto>.Fail("You must join the club first.");

            if (!ClubPermissionHelper.CanManage(member.Role))
                return ApiResponse<EventResponseDto>.Fail("Only admin or moderator can create events.");

            var newEvent = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                EventDate = dto.EventDate,
                ClubId = dto.ClubId,
                CreatedBy = userId
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return ApiResponse<EventResponseDto>.Ok(MapToResponse(newEvent, 0), "Event created successfully.");
        }

        public async Task<ApiResponse<EventResponseDto>> UpdateAsync(int userId, int eventId, CreateEventDto dto)
        {
            var eventData = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (eventData == null)
                return ApiResponse<EventResponseDto>.Fail("Event not found.");

            var member = await _context.GetMembershipAsync(userId, eventData.ClubId);
            var isCreator = eventData.CreatedBy == userId;
            var canManage = member != null && ClubPermissionHelper.CanManage(member.Role);

            if (!isCreator && !canManage)
                return ApiResponse<EventResponseDto>.Fail("You are not allowed to update this event.");

            if (eventData.ClubId != dto.ClubId)
            {
                var targetMember = await _context.GetMembershipAsync(userId, dto.ClubId);

                if (targetMember == null || !ClubPermissionHelper.CanManage(targetMember.Role))
                    return ApiResponse<EventResponseDto>.Fail("You are not a manager of the target club.");
            }

            if (dto.EventDate <= DateTime.UtcNow)
                return ApiResponse<EventResponseDto>.Fail("Event date must be in the future.");

            eventData.Title = dto.Title;
            eventData.Description = dto.Description;
            eventData.EventDate = dto.EventDate;
            eventData.ClubId = dto.ClubId;

            await _context.SaveChangesAsync();

            var total = await _context.EventAttendances.CountAsync(x => x.EventId == eventId);
            return ApiResponse<EventResponseDto>.Ok(MapToResponse(eventData, total), "Event updated successfully.");
        }

        public async Task<ApiResponse<string>> DeleteAsync(int userId, int eventId)
        {
            var eventData = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (eventData == null)
                return ApiResponse<string>.Fail("Event not found.");

            var member = await _context.GetMembershipAsync(userId, eventData.ClubId);
            var isCreator = eventData.CreatedBy == userId;
            var canManage = member != null && ClubPermissionHelper.CanManage(member.Role);

            if (!isCreator && !canManage)
                return ApiResponse<string>.Fail("You are not allowed to delete this event.");

            _context.EventAttendances.RemoveRange(
                _context.EventAttendances.Where(x => x.EventId == eventId));

            _context.EventJoinRequests.RemoveRange(
                _context.EventJoinRequests.Where(x => x.EventId == eventId));

            _context.Events.Remove(eventData);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Event deleted successfully.");
        }

        public async Task<ApiResponse<string>> JoinAsync(int userId, int eventId)
        {
            var eventInfo = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (eventInfo == null)
                return ApiResponse<string>.Fail("Event not found.");

            if (eventInfo.EventDate <= DateTime.UtcNow)
                return ApiResponse<string>.Fail("This event has already passed.");

            var isClubMember = await _context.IsMemberAsync(userId, eventInfo.ClubId);
            if (!isClubMember)
                return ApiResponse<string>.Fail("You must join the club first.");

            var alreadyJoined = await _context.EventAttendances
                .AnyAsync(x => x.EventId == eventId && x.UserId == userId);
            if (alreadyJoined)
                return ApiResponse<string>.Fail("You have already joined this event.");

            var hasPendingRequest = await _context.EventJoinRequests
                .AnyAsync(x => x.EventId == eventId && x.UserId == userId && x.Status == JoinRequestStatus.Pending);
            if (hasPendingRequest)
                return ApiResponse<string>.Fail("You already have a pending join request for this event.");

            var joinRequest = new EventJoinRequest
            {
                EventId = eventId,
                UserId = userId,
                Status = JoinRequestStatus.Pending
            };
            _context.EventJoinRequests.Add(joinRequest);
            await _context.SaveChangesAsync();

            if (eventInfo.CreatedBy != userId)
            {
                await _notificationService.CreateAndPushAsync(new CreateNotificationDto
                {
                    SenderId = userId,
                    ReceiverId = eventInfo.CreatedBy,
                    Type = NotificationType.EventJoin,
                    Message = "Someone requested to join your event."

                });
            }

            return ApiResponse<string>.Ok($"Your request to join event '{eventInfo.Title}' has been submitted and is pending approval.");
        }

        public async Task<ApiResponse<string>> LeaveAsync(int userId, int eventId)
        {
            var eventInfo = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (eventInfo == null)
                return ApiResponse<string>.Fail("Event not found.");

            if (eventInfo.CreatedBy == userId)
                return ApiResponse<string>.Fail("The event creator cannot leave their own event.");

            var attendance = await _context.EventAttendances
                .FirstOrDefaultAsync(x => x.EventId == eventId && x.UserId == userId);

            if (attendance != null)
            {
                _context.EventAttendances.Remove(attendance);
                await _context.SaveChangesAsync();
                return ApiResponse<string>.Ok("Left event successfully.");
            }

            // Allow cancelling a still-pending join request too.
            var pendingRequest = await _context.EventJoinRequests
                .FirstOrDefaultAsync(x => x.EventId == eventId && x.UserId == userId && x.Status == JoinRequestStatus.Pending);

            if (pendingRequest != null)
            {
                _context.EventJoinRequests.Remove(pendingRequest);
                await _context.SaveChangesAsync();
                return ApiResponse<string>.Ok("Join request cancelled.");
            }

            return ApiResponse<string>.Fail("You have not joined this event.");
        }

        public async Task<ApiResponse<EventJoinStatusDto>> GetJoinStatusAsync(int userId, int eventId)
        {
            var exists = await _context.Events.AnyAsync(x => x.Id == eventId);
            if (!exists)
                return ApiResponse<EventJoinStatusDto>.Fail("Event not found.");

            var attendance = await _context.EventAttendances
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EventId == eventId && x.UserId == userId);

            return ApiResponse<EventJoinStatusDto>.Ok(new EventJoinStatusDto
            {
                EventId = eventId,
                HasJoined = attendance != null,
                JoinedAt = attendance?.JoinedAt
            });
        }

        public async Task<ApiResponse<List<EventJoinRequestDto>>> GetJoinRequestsAsync(int userId, int eventId)
        {
            var eventData = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (eventData == null)
                return ApiResponse<List<EventJoinRequestDto>>.Fail("Event not found.");

            var member = await _context.GetMembershipAsync(userId, eventData.ClubId);
            var isCreator = eventData.CreatedBy == userId;
            var canManage = member != null && ClubPermissionHelper.CanManage(member.Role);

            if (!isCreator && !canManage)
                return ApiResponse<List<EventJoinRequestDto>>.Fail("Only the event creator or club admin/moderator can view join requests.");

            var requests = await _context.EventJoinRequests
                .AsNoTracking()
                .Where(x => x.EventId == eventId && x.Status == JoinRequestStatus.Pending)
                .OrderBy(x => x.RequestedAt)
                .Select(x => new EventJoinRequestDto
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    UserId = x.UserId,
                    UserName = x.User != null ? x.User.Name : null,
                    Status = x.Status.ToString(),
                    RequestedAt = x.RequestedAt
                })
                .ToListAsync();

            return ApiResponse<List<EventJoinRequestDto>>.Ok(requests);
        }

        public async Task<ApiResponse<string>> RespondToJoinRequestAsync(int moderatorId, int eventId, int requestId, bool approve)
        {
            var eventData = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (eventData == null)
                return ApiResponse<string>.Fail("Event not found.");

            var member = await _context.GetMembershipAsync(moderatorId, eventData.ClubId);
            var isCreator = eventData.CreatedBy == moderatorId;
            var canManage = member != null && ClubPermissionHelper.CanManage(member.Role);

            if (!isCreator && !canManage)
                return ApiResponse<string>.Fail("Only the event creator or club admin/moderator can respond to join requests.");

            var request = await _context.EventJoinRequests
                .FirstOrDefaultAsync(x => x.Id == requestId && x.EventId == eventId);

            if (request == null)
                return ApiResponse<string>.Fail("Join request not found.");

            if (request.Status != JoinRequestStatus.Pending)
                return ApiResponse<string>.Fail("This join request has already been responded to.");

            request.Status = approve ? JoinRequestStatus.Approved : JoinRequestStatus.Rejected;
            request.RespondedBy = moderatorId;
            request.RespondedAt = DateTime.UtcNow;

            if (approve)
            {
                var alreadyAttending = await _context.EventAttendances
                    .AnyAsync(x => x.EventId == eventId && x.UserId == request.UserId);

                if (!alreadyAttending)
                {
                    _context.EventAttendances.Add(new EventAttendance
                    {
                        EventId = eventId,
                        UserId = request.UserId
                    });
                }
            }

            await _context.SaveChangesAsync();

            await _notificationService.CreateAndPushAsync(new CreateNotificationDto
            {
                SenderId = moderatorId,
                ReceiverId = request.UserId,
                Type = NotificationType.EventJoin,
                Message = approve
                    ? $"Your request to join event '{eventData.Title}' was approved."
                    : $"Your request to join event '{eventData.Title}' was rejected."
            });

            return ApiResponse<string>.Ok(approve ? "Join request approved." : "Join request rejected.");
        }

        public async Task<ApiResponse<PagedResultDto<EventSummaryDto>>> GetAllAsync(int page, int pageSize)
        {
            var query = _context.Events
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new EventSummaryDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    EventDate = x.EventDate,
                    ClubId = x.ClubId,
                    CreatedAt = x.CreatedAt,
                    TotalAttendees = x.Attendances.Count
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<EventSummaryDto>>.Ok(result);
        }

        public async Task<ApiResponse<EventResponseDto>> GetByIdAsync(int eventId)
        {
            var data = await _context.Events
                .AsNoTracking()
                .Where(x => x.Id == eventId)
                .Select(x => new EventResponseDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    EventDate = x.EventDate,
                    ClubId = x.ClubId,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    TotalAttendees = x.Attendances.Count
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return ApiResponse<EventResponseDto>.Fail("Event not found.");

            return ApiResponse<EventResponseDto>.Ok(data);
        }

        public async Task<ApiResponse<PagedResultDto<EventSummaryDto>>> GetUpcomingAsync(int page, int pageSize)
        {
            var query = _context.Events
                .AsNoTracking()
                .Where(x => x.EventDate > DateTime.UtcNow)
                .OrderBy(x => x.EventDate)
                .Select(x => new EventSummaryDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    EventDate = x.EventDate,
                    ClubId = x.ClubId,
                    CreatedAt = x.CreatedAt,
                    TotalAttendees = x.Attendances.Count
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<EventSummaryDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<EventSummaryDto>>> SearchAsync(
            string keyword, int? clubId, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return ApiResponse<PagedResultDto<EventSummaryDto>>.Fail("Keyword is required.");

            keyword = keyword.Trim().ToLower();

            var query = _context.Events.AsNoTracking()
                .Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));

            if (clubId.HasValue)
                query = query.Where(x => x.ClubId == clubId.Value);

            var projected = query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new EventSummaryDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    EventDate = x.EventDate,
                    ClubId = x.ClubId,
                    CreatedAt = x.CreatedAt,
                    TotalAttendees = x.Attendances.Count
                });

            var result = await PaginationHelper.ToPagedResultAsync(projected, page, pageSize);
            return ApiResponse<PagedResultDto<EventSummaryDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<EventSummaryDto>>> GetByClubAsync(int clubId, int page, int pageSize)
        {
            var clubExists = await _context.Clubs.AnyAsync(x => x.Id == clubId);
            if (!clubExists)
                return ApiResponse<PagedResultDto<EventSummaryDto>>.Fail("Club not found.");

            var query = _context.Events
                .AsNoTracking()
                .Where(x => x.ClubId == clubId)
                .OrderByDescending(x => x.EventDate)
                .Select(x => new EventSummaryDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    EventDate = x.EventDate,
                    ClubId = x.ClubId,
                    CreatedAt = x.CreatedAt,
                    TotalAttendees = x.Attendances.Count
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<EventSummaryDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<EventSummaryDto>>> GetMyEventsAsync(
            int userId, int page, int pageSize)
        {
            var query = _context.Events
                .AsNoTracking()
                .Where(x => x.CreatedBy == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new EventSummaryDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    EventDate = x.EventDate,
                    ClubId = x.ClubId,
                    CreatedAt = x.CreatedAt,
                    TotalAttendees = x.Attendances.Count
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<EventSummaryDto>>.Ok(result);
        }

        public async Task<ApiResponse<PagedResultDto<MyJoinedEventDto>>> GetMyJoinedEventsAsync(
            int userId, int page, int pageSize)
        {
            var now = DateTime.UtcNow;

            var query = _context.EventAttendances
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.JoinedAt)
                .Select(x => new MyJoinedEventDto
                {
                    EventId = x.EventId,
                    EventTitle = x.Event != null ? x.Event.Title : null,
                    EventDescription = x.Event != null ? x.Event.Description : null,
                    EventDate = x.Event != null ? x.Event.EventDate : default,
                    ClubId = x.Event != null ? x.Event.ClubId : 0,
                    JoinedAt = x.JoinedAt,
                    IsUpcoming = x.Event != null && x.Event.EventDate > now
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<MyJoinedEventDto>>.Ok(result);
        }

        public async Task<ApiResponse<List<ClubUpcomingEventDto>>> GetMyClubsUpcomingAsync(int userId)
        {
            var now = DateTime.UtcNow;

            var myClubIds = await _context.ClubMembers
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.ClubId)
                .ToListAsync();

            if (!myClubIds.Any())
                return ApiResponse<List<ClubUpcomingEventDto>>.Ok(new List<ClubUpcomingEventDto>());

            var events = await _context.Events
                .AsNoTracking()
                .Where(x => myClubIds.Contains(x.ClubId) && x.EventDate > now)
                .OrderBy(x => x.EventDate)
                .Select(x => new ClubUpcomingEventDto
                {
                    EventId = x.Id,
                    Title = x.Title,
                    EventDate = x.EventDate,
                    ClubId = x.ClubId,
                    ClubName = _context.Clubs
                                        .Where(c => c.Id == x.ClubId)
                                        .Select(c => c.Name)
                                        .FirstOrDefault(),
                    TotalAttendees = x.Attendances.Count
                })
                .ToListAsync();

            return ApiResponse<List<ClubUpcomingEventDto>>.Ok(events);
        }

        public async Task<ApiResponse<List<EventAttendeeDto>>> GetAttendeesAsync(int eventId)
        {
            var exists = await _context.Events.AnyAsync(x => x.Id == eventId);
            if (!exists)
                return ApiResponse<List<EventAttendeeDto>>.Fail("Event not found.");

            var attendees = await _context.EventAttendances
                .AsNoTracking()
                .Where(x => x.EventId == eventId)
                .Select(x => new EventAttendeeDto
                {
                    EventId = x.EventId,
                    EventTitle = x.Event != null ? x.Event.Title : null,
                    UserId = x.UserId,
                    UserName = x.User != null ? x.User.Name : null,
                    UserEmail = x.User != null ? x.User.Email : null,
                    JoinedAt = x.JoinedAt
                })
                .ToListAsync();

            return ApiResponse<List<EventAttendeeDto>>.Ok(attendees);
        }

        public async Task<ApiResponse<EventStatsDto>> GetStatsAsync(int userId, int eventId)
        {
            var eventData = await _context.Events
                .AsNoTracking()
                .Where(x => x.Id == eventId)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.EventDate,
                    x.ClubId,
                    x.CreatedBy,
                    TotalAttendees = x.Attendances.Count
                })
                .FirstOrDefaultAsync();

            if (eventData == null)
                return ApiResponse<EventStatsDto>.Fail("Event not found.");

            var member = await _context.GetMembershipAsync(userId, eventData.ClubId);
            var isCreator = eventData.CreatedBy == userId;
            var canManage = member != null && ClubPermissionHelper.CanManage(member.Role);

            if (!isCreator && !canManage)
                return ApiResponse<EventStatsDto>.Fail("Access denied.");

            return ApiResponse<EventStatsDto>.Ok(new EventStatsDto
            {
                EventId = eventData.Id,
                Title = eventData.Title,
                EventDate = eventData.EventDate,
                TotalAttendees = eventData.TotalAttendees,
                IsUpcoming = eventData.EventDate > DateTime.UtcNow
            });
        }

        private static EventResponseDto MapToResponse(Event e, int totalAttendees) => new()
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            EventDate = e.EventDate,
            ClubId = e.ClubId,
            CreatedBy = e.CreatedBy,
            CreatedAt = e.CreatedAt,
            TotalAttendees = totalAttendees
        };
    }
}