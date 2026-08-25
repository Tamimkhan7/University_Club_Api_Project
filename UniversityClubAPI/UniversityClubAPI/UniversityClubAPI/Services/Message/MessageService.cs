using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Message;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Hubs;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Services.MessageService
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly INotificationService _notificationService;
        private readonly ILogger<MessageService> _logger;

        private const int EditWindowMinutes = 15;

        public MessageService(AppDbContext context, IHubContext<ChatHub> chatHub, INotificationService notificationService, ILogger<MessageService> logger)
        {
            _context = context;
            _chatHub = chatHub;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<MessageResponseDto> SendAsync(int senderId, SendMessageDto dto)
        {
            if (dto.ReceiverId == senderId)
                throw new InvalidOperationException("Cannot message yourself");

            var receiver = await _context.Users.FirstOrDefaultAsync(x => x.Id == dto.ReceiverId)
                ?? throw new KeyNotFoundException("Receiver not found");

            var blocked = await _context.BlockedUsers.AnyAsync(x =>
                (x.BlockerId == senderId && x.BlockedUserId == dto.ReceiverId) ||
                (x.BlockerId == dto.ReceiverId && x.BlockedUserId == senderId));
            if (blocked)
                throw new InvalidOperationException("Messaging is blocked between these users");

            var sender = await _context.Users.FirstOrDefaultAsync(x => x.Id == senderId);

            var msg = new Message
            {
                SenderId = senderId,
                ReceiverID = dto.ReceiverId,
                Text = dto.Text!.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _context.Messages.Add(msg);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MsgId} sent from user {SenderId} to user {ReceiverId}",
                msg.Id, senderId, dto.ReceiverId);

            msg.Sender = sender;
            var responseDto = MapToDto(msg);

            await _chatHub.Clients.User(dto.ReceiverId.ToString())
                .SendAsync("ReceiveMessage", responseDto);

            await _notificationService.CreateAndPushAsync(new CreateNotificationDto
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                Type = NotificationType.Message,
                Message = "Sent you a message"
            });

            return responseDto;
        }

        public async Task<PagedResultDto<MessageResponseDto>> GetChatAsync(int currentUserId, int otherUserId, MessageQueryDto query)
        {
            var q = _context.Messages
                .AsNoTracking()
                .Include(x => x.Sender)
                .Where(x =>
                    !x.IsDeletedForEveryone &&
                    !(x.SenderId == currentUserId && x.IsDeletedBySender) &&
                    !(x.ReceiverID == currentUserId && x.IsDeletedByReceiver) &&
                    (
                        (x.SenderId == currentUserId && x.ReceiverID == otherUserId) ||
                        (x.SenderId == otherUserId && x.ReceiverID == currentUserId)
                    ));

            if (query.From.HasValue) q = q.Where(x => x.CreatedAt >= query.From.Value);
            if (query.To.HasValue) q = q.Where(x => x.CreatedAt <= query.To.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
                q = q.Where(x => x.Text != null && x.Text.Contains(query.Search));

            q = query.SortOrder.ToLower() == "desc"
                ? q.OrderByDescending(x => x.CreatedAt)
                : q.OrderBy(x => x.CreatedAt);

            var paged = await PaginationHelper.ToPagedResultAsync(q, query);

            return new PagedResultDto<MessageResponseDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(MapToDto).ToList()
            };
        }

        public async Task<List<ConversationDto>> GetConversationsAsync(int currentUserId)
        {
            var messages = await _context.Messages
                .AsNoTracking()
                .Include(x => x.Sender)
                .Include(x => x.Receiver)
                .Where(x =>
                    (x.SenderId == currentUserId || x.ReceiverID == currentUserId) &&
                    !x.IsDeletedForEveryone &&
                    !(x.SenderId == currentUserId && x.IsDeletedBySender) &&
                    !(x.ReceiverID == currentUserId && x.IsDeletedByReceiver))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var conversations = messages
                .GroupBy(x => x.SenderId == currentUserId ? x.ReceiverID : x.SenderId)
                .Select(g =>
                {
                    var lastMsg = g.First();
                    var otherUserId = g.Key;
                    var otherUser = lastMsg.SenderId == currentUserId ? lastMsg.Receiver : lastMsg.Sender;

                    var unreadCount = g.Count(x => x.ReceiverID == currentUserId && !x.IsSeen);

                    return new ConversationDto
                    {
                        UserId = otherUserId,
                        UserName = otherUser?.Name ?? string.Empty,
                        ProfileImage = otherUser?.ProfileImage,
                        LastMessage = lastMsg.IsDeletedForEveryone
                            ? "This message was deleted"
                            : (lastMsg.MediaType == MessageMediaType.Voice ? "🎤 Voice message" : lastMsg.Text),
                        LastMessageAt = lastMsg.CreatedAt,
                        UnreadCount = unreadCount,
                        IsOnline = otherUser?.IsOnline ?? false
                    };
                })
                .OrderByDescending(x => x.LastMessageAt)
                .ToList();

            return conversations;
        }

        public async Task<int> GetUnreadCountAsync(int currentUserId)
        {
            return await _context.Messages
                .CountAsync(x =>
                    x.ReceiverID == currentUserId &&
                    !x.IsSeen &&
                    !x.IsDeletedForEveryone &&
                    !x.IsDeletedByReceiver);
        }

        public async Task<PagedResultDto<MessageResponseDto>> SearchMessagesAsync(int currentUserId, string keyword, PaginationParamsDto pagination)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return EmptyPaged(pagination);

            var q = _context.Messages
                .AsNoTracking()
                .Include(x => x.Sender)
                .Where(x =>
                    (x.SenderId == currentUserId || x.ReceiverID == currentUserId) &&
                    !x.IsDeletedForEveryone &&
                    !(x.SenderId == currentUserId && x.IsDeletedBySender) &&
                    !(x.ReceiverID == currentUserId && x.IsDeletedByReceiver) &&
                    x.Text != null && x.Text.Contains(keyword))
                .OrderByDescending(x => x.CreatedAt);

            var paged = await PaginationHelper.ToPagedResultAsync(q, pagination);

            return new PagedResultDto<MessageResponseDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(MapToDto).ToList()
            };
        }

        public async Task<MessageResponseDto> EditAsync(int messageId, int userId, EditMessageDto dto)
        {
            var message = await _context.Messages
                .Include(x => x.Sender)
                .FirstOrDefaultAsync(x => x.Id == messageId && x.SenderId == userId)
                ?? throw new KeyNotFoundException("Message not found");

            if (message.IsDeletedForEveryone)
                throw new InvalidOperationException("Cannot edit a deleted message");

            if ((DateTime.UtcNow - message.CreatedAt).TotalMinutes > EditWindowMinutes)
                throw new InvalidOperationException($"Edit window of {EditWindowMinutes} minutes has expired");

            message.Text = dto.Text.Trim();
            message.IsEdited = true;
            message.EditedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} edited by user {UserId}", messageId, userId);

            var responseDto = MapToDto(message);

            await _chatHub.Clients.User(message.ReceiverID.ToString())
                .SendAsync("MessageEdited", responseDto);

            return responseDto;
        }

        public async Task DeleteForEveryoneAsync(int messageId, int userId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(x => x.Id == messageId && x.SenderId == userId)
                ?? throw new KeyNotFoundException("Message not found");

            message.IsDeletedForEveryone = true;
            message.Text = "This message was deleted";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} deleted for everyone by user {UserId}", messageId, userId);

            await _chatHub.Clients.User(message.ReceiverID.ToString())
                .SendAsync("MessageDeleted", new { messageId, deletedForEveryone = true });
        }

        public async Task DeleteForMeAsync(int messageId, int userId)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(x => x.Id == messageId)
                ?? throw new KeyNotFoundException("Message not found");

            if (message.SenderId != userId && message.ReceiverID != userId)
                throw new KeyNotFoundException("Message not found");

            if (message.SenderId == userId)
                message.IsDeletedBySender = true;
            else
                message.IsDeletedByReceiver = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} deleted for self by user {UserId}", messageId, userId);
        }

        public async Task MarkAsSeenAsync(int currentUserId, int senderId)
        {
            var unseen = await _context.Messages
                .Where(x => x.SenderId == senderId && x.ReceiverID == currentUserId && !x.IsSeen)
                .ToListAsync();

            if (!unseen.Any()) return;

            foreach (var m in unseen) m.IsSeen = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {CurrentUserId} marked {Count} messages from user {SenderId} as seen",
                currentUserId, unseen.Count, senderId);

            await _chatHub.Clients.User(senderId.ToString())
                .SendAsync("MessagesSeen", new
                {
                    seenBy = currentUserId,
                    messageIds = unseen.Select(x => x.Id).ToList()
                });
        }

        private static MessageResponseDto MapToDto(Message m) => new()
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = m.Sender?.Name ?? string.Empty,
            SenderProfileImage = m.Sender?.ProfileImage,
            ReceiverId = m.ReceiverID,
            Text = m.IsDeletedForEveryone ? "This message was deleted" : m.Text,
            MediaType = m.MediaType,
            MediaUrl = m.IsDeletedForEveryone ? null : m.MediaUrl,
            DurationSeconds = m.DurationSeconds,
            IsSeen = m.IsSeen,
            IsEdited = m.IsEdited,
            IsDeletedForEveryone = m.IsDeletedForEveryone,
            CreatedAt = m.CreatedAt,
            EditedAt = m.EditedAt
        };

        private static PagedResultDto<MessageResponseDto> EmptyPaged(PaginationParamsDto p) => new()
        {
            Page = p.Page,
            PageSize = p.PageSize,
            TotalCount = 0,
            TotalPages = 0,
            Items = []
        };
    }
}