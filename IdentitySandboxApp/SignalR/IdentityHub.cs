using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace IdentitySandboxApp.SignalR
{
    [Authorize]
    public class IdentityHub : Hub
    {
        public async Task OnSend(string msg)
        {
            await Clients.All.SendAsync("send", msg, Context.User.Identity.Name);
        }

        public async Task Private(string msg, string user)
        {
            await Clients.User(user).SendAsync("send", msg, Context.User.Identity.Name, true);
        }
    }
}
