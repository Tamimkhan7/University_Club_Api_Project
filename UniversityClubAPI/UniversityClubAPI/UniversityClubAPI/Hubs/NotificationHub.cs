using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;
using UniversityClubAPI.Data;

namespace UniversityClubAPI.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly AppDbContext _context;

        private static readonly ConcurrentDictionary<int, int> ActiveConnectionCounts = new();

        public NotificationHub(ILogger<NotificationHub> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        private string? GetUserId()
           => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private int? GetUserIdInt()
            => int.TryParse(GetUserId(), out var id) ? id : null;

        private static string UserGroup(string? userId) => $"notification-{userId}";
        private static string PostGroup(string postId) => $"post-{postId}";
        private static string PresenceGroup(int userId) => $"presence-{userId}";

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId is not null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
                _logger.LogDebug("NotificationHub: user {UserId} connected ({ConnId})",
                    userId, Context.ConnectionId);
            }

            var userIdInt = GetUserIdInt();
            if (userIdInt is not null)
            {
                var newCount = ActiveConnectionCounts.AddOrUpdate(userIdInt.Value, 1, (_, count) => count + 1);

                if (newCount == 1)
                {
                    try
                    {
                        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userIdInt.Value);
                        if (user != null)
                        {
                            user.IsOnline = true;
                            await _context.SaveChangesAsync();
                        }

                        await Clients.Group(PresenceGroup(userIdInt.Value)).SendAsync("UserPresenceChanged", new
                        {
                            userId = userIdInt.Value,
                            isOnline = true,
                            lastSeenAt = (DateTime?)null
                        });
                    }
                    catch (Exception ex)
                    {

                        _logger.LogError(ex, "NotificationHub: failed to mark user {UserId} online", userIdInt.Value);
                    }
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId is not null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(userId));
                _logger.LogDebug("NotificationHub: user {UserId} disconnected ({ConnId})",
                    userId, Context.ConnectionId);
            }

            var userIdInt = GetUserIdInt();

            if (userIdInt is not null && ActiveConnectionCounts.ContainsKey(userIdInt.Value))
            {
                var newCount = ActiveConnectionCounts.AddOrUpdate(userIdInt.Value, 0, (_, count) => Math.Max(0, count - 1));

                if (newCount == 0)
                {
                    ActiveConnectionCounts.TryRemove(userIdInt.Value, out _);

                    try
                    {
                        var lastSeen = DateTime.UtcNow;
                        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userIdInt.Value);
                        if (user != null)
                        {
                            user.IsOnline = false;
                            user.LastSeenAt = lastSeen;
                            await _context.SaveChangesAsync();
                        }

                        await Clients.Group(PresenceGroup(userIdInt.Value)).SendAsync("UserPresenceChanged", new
                        {
                            userId = userIdInt.Value,
                            isOnline = false,
                            lastSeenAt = lastSeen
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "NotificationHub: failed to mark user {UserId} offline", userIdInt.Value);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinPost(string postId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, PostGroup(postId));
        }

        public async Task LeavePost(string postId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, PostGroup(postId));
        }

        public async Task WatchPresence(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, PresenceGroup(userId));
        }

        public async Task UnwatchPresence(int userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, PresenceGroup(userId));
        }
    }
}