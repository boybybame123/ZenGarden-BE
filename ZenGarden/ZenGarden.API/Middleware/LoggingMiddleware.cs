namespace ZenGarden.API.Middleware;

public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    private static async Task<string> ReadRequestBody(HttpRequest request, int maxLength = 1000)
    {
        if (request.Body == null || !request.Body.CanRead) 
            return "[Empty Body]";

        request.EnableBuffering(); 
        request.Body.Position = 0;

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var bodyText = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return string.IsNullOrWhiteSpace(bodyText) 
            ? "[Empty Body]" 
            : bodyText.Length > maxLength 
                ? string.Concat(bodyText.AsSpan(0, maxLength), "...") 
                : bodyText;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestBody = await ReadRequestBody(context.Request);

        if (context.Request.Path.Value is { } path && path.Contains("openai", StringComparison.OrdinalIgnoreCase))
            requestBody = "[FILTERED]";

        logger.LogInformation("[Request] {Method} {Path} | Body: {Body}",
            context.Request.Method, context.Request.Path, requestBody);

        await next(context);
    }
}
