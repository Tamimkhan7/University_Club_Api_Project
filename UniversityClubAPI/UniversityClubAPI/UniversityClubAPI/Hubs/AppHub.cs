using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace UniversityClubAPI.Hubs
{
    [Authorize]
    public class AppHub : Hub
    {
        public async Task SendMessage(string receiverId, string message)
        {
            var sender = Context.UserIdentifier;
            await Clients.User(receiverId)
                .SendAsync("ReceiveMessage", sender, message);
        }

        public async Task SendNotification(string receiverId, string message)
        {
            await Clients.User(receiverId)
                .SendAsync("ReceiveNotification", message);
        }

        public async Task Typing(string receiverId)
        {
            var sender = Context.UserIdentifier;

            await Clients.User(receiverId)
                .SendAsync("UserTyping", sender);
        }
    }
}