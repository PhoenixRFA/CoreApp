using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace MVCApp.SignalR
{
    public class NotificationHub : Hub<IClient>
    {
        public override async Task OnConnectedAsync()
        {
            //await Clients.All.SendAsync("debug", $"New connection: {Context.ConnectionId}");
            await Clients.All.Debug($"New connection: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            //await Clients.All.SendAsync("debug", $"{Context.ConnectionId} disconnected");
            await Clients.All.Debug($"{Context.ConnectionId} disconnected");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task BroadcastNotification(string msg)
        {
            //await Clients.All.SendAsync("notify", msg);
            await Clients.All.Notify(msg);
        }
    }

    public interface IClient
    {
        Task Notify(string msg);
        Task Debug(string msg);
    }
}
