using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Group;
using UniversityClubAPI.DTOs.Message;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.DTOs.VoiceMessage;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Hubs;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Services.VoiceMessageService
{
    public class VoiceMessageService : IVoiceMessageService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly IHubContext<GroupHub> _groupHub;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly INotificationService _notificationService;
        private readonly ILogger<VoiceMessageService> _logger;

        private static readonly string[] AllowedExtensions = { ".mp3", ".wav", ".m4a", ".ogg", ".webm", ".aac" };

        private static readonly string[] AllowedContentTypes =
        {
            "audio/mpeg", "audio/mp3", "audio/wav", "audio/x-wav", "audio/wave",
            "audio/mp4", "audio/x-m4a", "audio/aac", "audio/ogg", "audio/webm"
        };

        private const long MaxVoiceSizeBytes = 15 * 1024 * 1024;

        public VoiceMessageService(
            AppDbContext context,
            IWebHostEnvironment environment,
            IHubContext<ChatHub> chatHub,
            IHubContext<GroupHub> groupHub,
            IHubContext<NotificationHub> notificationHub,
            INotificationService notificationService,
            ILogger<VoiceMessageService> logger)
        {
            _context = context;
            _environment = environment;
            _chatHub = chatHub;
            _groupHub = groupHub;
            _notificationHub = notificationHub;
            _notificationService = notificationService;
            _logger = logger;
        }

        private string GetVoiceUploadFolder()
        {
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
                if (!Directory.Exists(webRoot))
                    Directory.CreateDirectory(webRoot);
            }

            var folder = Path.Combine(webRoot, "uploads", "voice");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        private async Task<(bool Ok, string? Url, string? Error)> SaveAudioFileAsync(IFormFile audio)
        {
            if (audio == null || audio.Length == 0)
                return (false, null, "Audio file is required.");

            if (audio.Length > MaxVoiceSizeBytes)
                return (false, null, "Audio file must be less than 15 MB.");

            var extension = Path.GetExtension(audio.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return (false, null, $"Audio format '{extension}' is not supported. Allowed: mp3, wav, m4a, ogg, webm, aac.");

            var contentType = audio.ContentType?.Split(';')[0].Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(contentType) || !AllowedContentTypes.Contains(contentType))
                return (false, null, "Invalid audio file content type.");

            var uploadFolder = GetVoiceUploadFolder();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            try
            {
                await using (var stream = new System.IO.FileStream(filePath, FileMode.Create))
                    await audio.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save voice file {FileName}", uniqueFileName);
                return (false, null, "Failed to save audio file. Please try again.");
            }

            return (true, "/uploads/voice/" + uniqueFileName, null);
        }

        private void TryDeletePhysicalFile(string? mediaUrl)
        {
            if (string.IsNullOrEmpty(mediaUrl))
                return;

            try
            {
                var fileName = Path.GetFileName(mediaUrl);
                var filePath = Path.Combine(GetVoiceUploadFolder(), fileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete physical voice file for {MediaUrl}", mediaUrl);
            }
        }

        public async Task<ApiResponse<MessageResponseDto>> SendDirectVoiceMessageAsync(
            int senderId, int receiverId, SendVoiceMessageDto dto)
        {
            if (receiverId == senderId)
                return ApiResponse<MessageResponseDto>.Fail("You cannot send a voice message to yourself.");

            var receiver = await _context.Users.FirstOrDefaultAsync(x => x.Id == receiverId);
            if (receiver == null)
                return ApiResponse<MessageResponseDto>.Fail("Receiver not found.");

            var blocked = await _context.BlockedUsers.AnyAsync(x =>
                (x.BlockerId == senderId && x.BlockedUserId == receiverId) ||
                (x.BlockerId == receiverId && x.BlockedUserId == senderId));

            if (blocked)
                return ApiResponse<MessageResponseDto>.Fail("Messaging is blocked between these users.");

            var (ok, url, error) = await SaveAudioFileAsync(dto.Audio);
            if (!ok)
                return ApiResponse<MessageResponseDto>.Fail(error!);

            var sender = await _context.Users.FirstOrDefaultAsync(x => x.Id == senderId);

            var msg = new Message
            {
                SenderId = senderId,
                ReceiverID = receiverId,
                MediaType = MessageMediaType.Voice,
                MediaUrl = url,
                DurationSeconds = dto.DurationSeconds,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(msg);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save direct voice message from {SenderId} to {ReceiverId}", senderId, receiverId);
                TryDeletePhysicalFile(url);
                return ApiResponse<MessageResponseDto>.Fail("Failed to send voice message. Please try again.");
            }

            _logger.LogInformation("Voice message {MsgId} sent from {SenderId} to {ReceiverId}",
                msg.Id, senderId, receiverId);

            var responseDto = new MessageResponseDto
            {
                Id = msg.Id,
                SenderId = senderId,
                SenderName = sender?.Name ?? string.Empty,
                SenderProfileImage = sender?.ProfileImage,
                ReceiverId = receiverId,
                Text = null,
                MediaType = MessageMediaType.Voice,
                MediaUrl = url,
                DurationSeconds = dto.DurationSeconds,
                IsSeen = false,
                IsEdited = false,
                IsDeletedForEveryone = false,
                CreatedAt = msg.CreatedAt
            };

            await _chatHub.Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", responseDto);

            await _notificationService.CreateAndPushAsync(new CreateNotificationDto
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Type = NotificationType.Message,
                Message = "Sent you a voice message"
            });

            return ApiResponse<MessageResponseDto>.Ok(responseDto, "Voice message sent successfully.");
        }

        public async Task<ApiResponse<GroupMessageDto>> SendGroupVoiceMessageAsync(
            int senderId, int groupId, SendVoiceMessageDto dto)
        {
            if (!await _context.Groups.AnyAsync(x => x.Id == groupId))
                return ApiResponse<GroupMessageDto>.Fail("Group not found.");

            var isMember = await _context.GroupMembers
                .AnyAsync(x => x.GroupId == groupId && x.UserId == senderId);

            if (!isMember)
                return ApiResponse<GroupMessageDto>.Fail("You are not a member of this group.");

            var (ok, url, error) = await SaveAudioFileAsync(dto.Audio);
            if (!ok)
                return ApiResponse<GroupMessageDto>.Fail(error!);

            var message = new GroupMessage
            {
                GroupId = groupId,
                SenderId = senderId,
                MediaType = MessageMediaType.Voice,
                MediaUrl = url,
                DurationSeconds = dto.DurationSeconds
            };

            _context.GroupMessages.Add(message);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save group voice message from {SenderId} in Group {GroupId}", senderId, groupId);
                TryDeletePhysicalFile(url);
                return ApiResponse<GroupMessageDto>.Fail("Failed to send voice message. Please try again.");
            }

            var senderName = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == senderId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync();

            var resultDto = new GroupMessageDto
            {
                Id = message.Id,
                GroupId = groupId,
                SenderId = senderId,
                SenderName = senderName,
                Text = null,
                MediaType = MessageMediaType.Voice,
                MediaUrl = url,
                DurationSeconds = dto.DurationSeconds,
                CreatedAt = message.CreatedAt
            };

            await _groupHub.Clients.Group($"group-{groupId}").SendAsync("ReceiveGroupMessage", resultDto);

            var otherMemberIds = await _context.GroupMembers
                .Where(x => x.GroupId == groupId && x.UserId != senderId)
                .Select(x => x.UserId)
                .ToListAsync();

            foreach (var memberId in otherMemberIds)
            {
                await _notificationHub.Clients.Group($"notification-{memberId}")
                    .SendAsync("NewGroupMessage", new
                    {
                        groupId,
                        senderId,
                        senderName,
                        preview = "🎤 Voice message"
                    });
            }

            _logger.LogInformation("Voice message {MsgId} sent by {SenderId} in Group {GroupId}",
                message.Id, senderId, groupId);

            return ApiResponse<GroupMessageDto>.Ok(resultDto, "Voice message sent successfully.");
        }

        public async Task<ApiResponse<bool>> DeleteDirectVoiceMessageAsync(int userId, int messageId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.MediaType == MessageMediaType.Voice);

            if (message == null)
                return ApiResponse<bool>.Fail("Voice message not found.");

            if (message.SenderId != userId)
                return ApiResponse<bool>.Fail("You can only delete your own voice messages.");

            if (message.IsDeletedForEveryone)
                return ApiResponse<bool>.Fail("This voice message is already deleted.");

            message.IsDeletedForEveryone = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete voice message {MessageId}", messageId);
                return ApiResponse<bool>.Fail("Failed to delete voice message. Please try again.");
            }

            TryDeletePhysicalFile(message.MediaUrl);

            await _chatHub.Clients.User(message.ReceiverID.ToString())
                .SendAsync("VoiceMessageDeleted", new { messageId = message.Id });

            _logger.LogInformation("Voice message {MsgId} deleted by {UserId}", messageId, userId);

            return ApiResponse<bool>.Ok(true, "Voice message deleted.");
        }

        public async Task<ApiResponse<bool>> DeleteGroupVoiceMessageAsync(int userId, int messageId)
        {
            var message = await _context.GroupMessages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.MediaType == MessageMediaType.Voice);

            if (message == null)
                return ApiResponse<bool>.Fail("Voice message not found.");

            if (message.SenderId != userId)
                return ApiResponse<bool>.Fail("You can only delete your own voice messages.");

            if (message.IsDeletedForEveryone)
                return ApiResponse<bool>.Fail("This voice message is already deleted.");

            message.IsDeletedForEveryone = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete group voice message {MessageId}", messageId);
                return ApiResponse<bool>.Fail("Failed to delete voice message. Please try again.");
            }

            TryDeletePhysicalFile(message.MediaUrl);

            await _groupHub.Clients.Group($"group-{message.GroupId}")
                .SendAsync("GroupVoiceMessageDeleted", new { messageId = message.Id, groupId = message.GroupId });

            _logger.LogInformation("Voice message {MsgId} deleted by {UserId} in Group {GroupId}",
                messageId, userId, message.GroupId);

            return ApiResponse<bool>.Ok(true, "Voice message deleted.");
        }
    }
}