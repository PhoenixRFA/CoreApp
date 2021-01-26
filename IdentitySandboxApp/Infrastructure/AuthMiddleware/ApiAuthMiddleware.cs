using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace IdentitySandboxApp.Infrastructure.AuthMiddleware
{
    public class ApiAuthMiddleware : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler DefaultHandler = new AuthorizationMiddlewareResultHandler();

        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                string ua = context.Request.Headers["User-Agent"].ToString();
                if (string.IsNullOrEmpty(ua))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync("useragent is not set");
                }
            }

            await DefaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
