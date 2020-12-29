using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MVCApp.Services;

namespace MVCApp.Middleware
{
    public class RequestStoreTestMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestStoreTestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IRequestStoreService requestStore)
        {
            requestStore.Add("id", Guid.NewGuid().ToString("N"));

            await _next(context);
        }
    }
}
