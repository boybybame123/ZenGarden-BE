using System.Diagnostics;

namespace ZenGarden.API.Middleware
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMiddleware> _logger;

        public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            _logger.LogInformation($"Request {context.Request.Method} {context.Request.Path} took {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}