using Microsoft.AspNetCore.Builder;

namespace CoreApp.Middleware
{
    public static class StopwatchMiddlewareExtension
    {
        public static IApplicationBuilder UseStopwatch(this IApplicationBuilder application, string name) =>
            application.UseMiddleware<StopwatchMiddleware>(name);
    }
}
