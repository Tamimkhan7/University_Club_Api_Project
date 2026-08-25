using Microsoft.AspNetCore.SignalR;

namespace UniversityClubAPI.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendPrivateMessage(string receiverId, object message)
        {
            await Clients.User(receiverId)
                .SendAsync("ReceiveMessage", message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                groupName
            );
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                groupName
            );
        }

        public async Task SendGroupMessage(string groupName, object message)
        {
            await Clients.Group(groupName)
                .SendAsync("ReceiveGroupMessage", message);
        }

        public async Task Typing(string receiverId)
        {
            await Clients.User(receiverId)
                .SendAsync("Typing");
        }

        public async Task GroupTyping(string groupName, string userName)
        {
            await Clients.Group(groupName)
                .SendAsync("GroupTyping", userName);
        }
    }
}