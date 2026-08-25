using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Hubs;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<NotificationDto> CreateAndPushAsync(CreateNotificationDto dto, bool allowSelfNotify = false)
        {
            if (dto.SenderId == dto.ReceiverId && !allowSelfNotify)
                throw new ArgumentException("Cannot send a notification to yourself");

            var notification = new Notification
            {
                SenderId = dto.SenderId,
                ReceiverId = dto.ReceiverId,
                Type = dto.Type,
                Message = dto.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var notifDto = MapToDto(notification);
            await PushToUserAsync(dto.ReceiverId, notifDto);

            return notifDto;
        }

        public async Task<PagedResultDto<NotificationDto>> GetPagedAsync(
            int userId,
            NotificationQueryDto query)
        {
            var q = _context.Notifications
                .AsNoTracking()
                .Where(x => x.ReceiverId == userId);

            if (!string.IsNullOrWhiteSpace(query.Type) && Enum.TryParse<NotificationType>(query.Type, true, out var parsedType))
            {
                q = q.Where(x => x.Type == parsedType);
            }

            q = q.OrderByDescending(x => x.CreatedAt);

            var paged = await PaginationHelper.ToPagedResultAsync(q, query.Page, query.PageSize);

            return new PagedResultDto<NotificationDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(MapToDto).ToList()
            };
        }

        public async Task<List<NotificationDto>> GetUnreadAsync(int userId)
        {
            var entities = await _context.Notifications
                .AsNoTracking()
                .Where(x => x.ReceiverId == userId && !x.IsRead)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return entities.Select(MapToDto).ToList();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(x => x.ReceiverId == userId && !x.IsRead);
        }

        public async Task<NotificationDto?> GetByIdAsync(int userId, int notificationId)
        {
            var entity = await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.ReceiverId == userId);

            return entity is null ? null : MapToDto(entity);
        }

        public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.ReceiverId == userId);

            if (notification is null) return false;

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<int> MarkSelectedAsReadAsync(int userId, List<int> notificationIds)
        {
            var notifications = await _context.Notifications
                .Where(x => x.ReceiverId == userId &&
                    notificationIds.Contains(x.Id) && !x.IsRead).ToListAsync();

            if (notifications.Count == 0) return 0;

            foreach (var n in notifications)
                n.IsRead = true;

            await _context.SaveChangesAsync();
            return notifications.Count;
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(x => x.ReceiverId == userId && !x.IsRead).ToListAsync();

            if (notifications.Count == 0) return 0;

            foreach (var n in notifications)
                n.IsRead = true;

            await _context.SaveChangesAsync();
            return notifications.Count;
        }

        public async Task<bool> DeleteAsync(int userId, int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.ReceiverId == userId);

            if (notification is null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteSelectedAsync(int userId, List<int> notificationIds)
        {
            var notifications = await _context.Notifications
                .Where(x => x.ReceiverId == userId && notificationIds.Contains(x.Id)).ToListAsync();

            if (notifications.Count == 0) return 0;

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return notifications.Count;
        }

        public async Task<int> DeleteAllAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(x => x.ReceiverId == userId).ToListAsync();

            if (notifications.Count == 0) return 0;

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
            return notifications.Count;
        }

        private async Task PushToUserAsync(int receiverId, NotificationDto dto)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"notification-{receiverId}")
                    .SendAsync("ReceiveNotification", dto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to push real-time notification to user {UserId}", receiverId);
            }
        }

        private static NotificationDto MapToDto(Notification n) => new()
        {
            Id = n.Id,
            SenderId = n.SenderId,
            ReceiverId = n.ReceiverId,
            Type = n.Type,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}