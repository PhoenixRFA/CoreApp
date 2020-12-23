using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoreApp.Middleware
{
    public class StopwatchMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _name;
        private readonly Stopwatch _sw;
        private readonly ILogger<StopwatchMiddleware> _logger;
        
        public StopwatchMiddleware(string name, RequestDelegate next, ILogger<StopwatchMiddleware> logger)
        {
            _name = name;
            _next = next;
            _sw = new Stopwatch();
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            _sw.Reset();
            _sw.Start();
            
            await _next(context);

            _sw.Stop();
            _logger.LogInformation("{StopwatchName} Time: {Time}ms", _name, _sw.ElapsedMilliseconds);
        }
    }
}
