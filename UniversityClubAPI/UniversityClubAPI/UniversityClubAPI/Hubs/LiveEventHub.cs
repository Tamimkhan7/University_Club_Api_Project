using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.LiveEvent;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Hubs
{
    [Authorize]
    public class LiveEventHub : Hub
    {
        private readonly AppDbContext _context;

        private static readonly ConcurrentDictionary<string, (int EventId, int UserId)> ActiveConnections = new();

        public LiveEventHub(AppDbContext context)
        {
            _context = context;
        }

        private int? GetUserId()
        {
            var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private static string RoomGroup(int eventId) => $"live-{eventId}";

        public async Task JoinLiveRoom(int eventId)
        {
            var userId = GetUserId();
            if (userId == null) return;

            var ev = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (ev == null) return;

            var isMember = await _context.ClubMembers
                .AnyAsync(x => x.UserId == userId.Value && x.ClubId == ev.ClubId);
            if (!isMember) return;


            var moderation = await _context.LiveModerations
                .FirstOrDefaultAsync(x => x.EventId == eventId && x.UserId == userId.Value);

            if (moderation?.IsBanned == true)
            {
                await Clients.Caller.SendAsync("JoinRejected", new { eventId, reason = "You have been banned from this live session." });
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(eventId));
            ActiveConnections[Context.ConnectionId] = (eventId, userId.Value);

            var openEntry = await _context.LiveParticipants
                .FirstOrDefaultAsync(x => x.EventId == eventId && x.UserId == userId.Value && x.LeftAt == null);

            if (openEntry == null)
            {
                _context.LiveParticipants.Add(new LiveParticipant
                {
                    EventId = eventId,
                    UserId = userId.Value
                });
                await _context.SaveChangesAsync();
            }

            var viewerCount = await _context.LiveParticipants
                .CountAsync(x => x.EventId == eventId && x.LeftAt == null);

            await Clients.Group(RoomGroup(eventId)).SendAsync("ViewerCountUpdated", new { eventId, viewerCount });


            await Clients.Caller.SendAsync("MuteStatusChanged", new { eventId, isMuted = moderation?.IsMuted == true });
        }

        public async Task LeaveLiveRoom(int eventId)
        {
            var userId = GetUserId();
            if (userId == null) return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, RoomGroup(eventId));
            ActiveConnections.TryRemove(Context.ConnectionId, out _);

            await CloseOpenParticipation(eventId, userId.Value);

            var viewerCount = await _context.LiveParticipants
                .CountAsync(x => x.EventId == eventId && x.LeftAt == null);

            await Clients.Group(RoomGroup(eventId)).SendAsync("ViewerCountUpdated", new { eventId, viewerCount });
        }

        public async Task SendLiveMessage(int eventId, string message)
        {
            var userId = GetUserId();
            if (userId == null || string.IsNullOrWhiteSpace(message)) return;

            var ev = await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId);
            if (ev == null || ev.LiveStatus != LiveStatus.Live) return;


            var moderation = await _context.LiveModerations
                .FirstOrDefaultAsync(x => x.EventId == eventId && x.UserId == userId.Value);

            if (moderation?.IsMuted == true)
            {
                await Clients.Caller.SendAsync("MessageRejected", new { eventId, reason = "You are muted in this live session." });
                return;
            }

            message = message.Trim();
            if (message.Length > 500)
                message = message[..500];

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId.Value);

            var chatMessage = new LiveChatMessage
            {
                EventId = eventId,
                UserId = userId.Value,
                Message = message
            };

            _context.LiveChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            var dto = new LiveChatMessageDto
            {
                Id = chatMessage.Id,
                EventId = eventId,
                UserId = userId.Value,
                UserName = user?.Name,
                UserProfileImage = user?.ProfileImage,
                Message = chatMessage.Message,
                SentAt = chatMessage.SentAt
            };

            await Clients.Group(RoomGroup(eventId)).SendAsync("ReceiveLiveMessage", dto);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ActiveConnections.TryRemove(Context.ConnectionId, out var info))
            {
                await CloseOpenParticipation(info.EventId, info.UserId);

                var viewerCount = await _context.LiveParticipants
                    .CountAsync(x => x.EventId == info.EventId && x.LeftAt == null);

                await Clients.Group(RoomGroup(info.EventId))
                    .SendAsync("ViewerCountUpdated", new { eventId = info.EventId, viewerCount });
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task CloseOpenParticipation(int eventId, int userId)
        {
            var openEntry = await _context.LiveParticipants
                .Where(x => x.EventId == eventId && x.UserId == userId && x.LeftAt == null)
                .OrderByDescending(x => x.JoinedAt)
                .FirstOrDefaultAsync();

            if (openEntry != null)
            {
                openEntry.LeftAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}