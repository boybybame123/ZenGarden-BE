namespace ZenGarden.API.Middleware;

public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    private static async Task<string> ReadRequestBody(HttpRequest request, int maxLength = 1000)
    {
        if (request.ContentLength == null || request.ContentLength == 0 || !request.Body.CanRead)
            return "[Empty Body]";

        if (!request.Body.CanSeek)
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
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;

        var requestBody = path.Contains("openai", StringComparison.OrdinalIgnoreCase)
            ? "[FILTERED]"
            : await ReadRequestBody(context.Request);

        logger.LogInformation("[Request] {Method} {Path} | Body: {Body}", method, path, requestBody);

        await next(context);
    }
}