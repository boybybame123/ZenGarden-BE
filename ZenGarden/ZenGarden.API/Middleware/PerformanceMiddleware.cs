using System.Diagnostics;

namespace ZenGarden.API.Middleware;

public class PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger, IConfiguration config)
{
    private readonly int _slowThreshold = config.GetValue("Performance:SlowThreshold", 3000);

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await next(context);

        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (elapsedMs > _slowThreshold)
            logger.LogWarning("⚠️ Slow response from {Path}: {ElapsedMilliseconds}ms",
                context.Request.Path, elapsedMs);

        logger.LogInformation("📡 Request {Method} {Path} took {ElapsedMilliseconds} ms",
            context.Request.Method, context.Request.Path, elapsedMs);
    }
}