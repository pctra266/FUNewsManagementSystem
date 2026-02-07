using Microsoft.AspNetCore.SignalR;

namespace Presentation_API.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendArticleNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
