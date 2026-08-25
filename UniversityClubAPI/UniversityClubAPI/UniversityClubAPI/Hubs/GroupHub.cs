using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniversityClubAPI.Data;

namespace UniversityClubAPI.Hubs
{
    [Authorize]
    public class GroupHub : Hub
    {
        private readonly AppDbContext _context;
        public GroupHub(AppDbContext context)
        {
            _context = context;
        }

        private int? GetUserId()
        {
            var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }


        public async Task JoinGroupRoom(int groupId)
        {
            var userId = GetUserId();
            if (userId == null) return;

            var isMember = await _context.GroupMembers
                .AsNoTracking()
                .AnyAsync(x => x.GroupId == groupId && x.UserId == userId.Value);

            if (!isMember) return;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{groupId}");
        }


        public async Task LeaveGroupRoom(int groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group-{groupId}");
        }

        public async Task Typing(int groupId)
        {
            var userId = GetUserId();
            if (userId == null) return;

            await Clients.OthersInGroup($"group-{groupId}")
                .SendAsync("GroupTyping", new { groupId, userId });
        }

        public async Task StopTyping(int groupId)
        {
            var userId = GetUserId();
            if (userId == null) return;

            await Clients.OthersInGroup($"group-{groupId}")
                 .SendAsync("GroupTyping", new { groupId, userId });
        }
    }
}
