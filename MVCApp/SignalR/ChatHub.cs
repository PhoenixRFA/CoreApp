using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace MVCApp.SignalR
{
    public class ChatHub : Hub
    {
        public async Task OnSend(string msg, string user)
        {
            //await Clients.All.SendCoreAsync("send", new []{ msg });
            await Clients.All.SendAsync("send", msg, user);
        }

        public async Task Send()
        {
            HttpContext httpContext = Context.GetHttpContext();

            var data = new
            {
                ConnectionId = Context.ConnectionId,
                Claims = Context.User?.Claims,
                UserIdentifier = Context.UserIdentifier,
                Items = Context.Items.ToArray(),
                Features = Context.Features.Select(x=>x.Key.ToString()).ToArray(),
                Host = httpContext.Request.Host
            };

            await Clients.All.SendAsync("test", data);
        }

        public void Abort()
        {
            Context.Abort();
        }

        public async Task Object(User item)
        {
            await Clients.All.SendAsync("test", item);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("send", $"New connection: {Context.ConnectionId}", "system");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.All.SendAsync("send", $"{Context.ConnectionId} disconnected", "system");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
