using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CoreApp
{
    public class AltStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(context => context.Response.WriteAsync("Test AltStartup"));
        }
    }
}
