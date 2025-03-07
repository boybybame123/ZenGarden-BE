using System.Diagnostics;

namespace ZenGarden.API.Middleware;

public class PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await next(context);
        stopwatch.Stop();
        if (stopwatch.ElapsedMilliseconds > 3000)
            logger.LogWarning("Slow response from {Path}: {ElapsedMilliseconds}ms",
                context.Request.Path, stopwatch.ElapsedMilliseconds);
        logger.LogInformation(
            "Request {Method} {Path} took {ElapsedMilliseconds} ms",
            context.Request.Method,
            context.Request.Path,
            stopwatch.ElapsedMilliseconds);
    }
}