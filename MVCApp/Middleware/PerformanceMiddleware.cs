using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MVCApp.Middleware
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Stopwatch _sw;
        private readonly ILogger<PerformanceMiddleware> _logger;
        
        public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
        {
            _next = next;
            _sw = new Stopwatch();
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _sw.Start();
            
            await _next.Invoke(context);

            _sw.Stop();
            
            _logger.LogInformation("Request {Path}; Time elapsed: {TimeMs}ms", context.Request.Path, _sw.ElapsedMilliseconds);

            _sw.Reset();
        }
    }
}
