using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace MVCApp.SignalR
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("debug", $"New connection: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.All.SendAsync("debug", $"{Context.ConnectionId} disconnected");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task BroadcastNotification(string msg)
        {
            await Clients.All.SendAsync("notify", msg);
        }
    }
}
