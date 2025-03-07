namespace ZenGarden.API.Middleware;

public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    private static async Task<string> ReadRequestBody(HttpRequest request, int maxLength = 1000)
    {
        request.EnableBuffering();
        var bodyStream = new StreamReader(request.Body);
        var bodyText = await bodyStream.ReadToEndAsync();
        request.Body.Position = 0;
        return bodyText.Length > maxLength ? string.Concat(bodyText.AsSpan(0, maxLength), "...") : bodyText;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestBody = await ReadRequestBody(context.Request);

        if (context.Request.Path.Value != null &&
            context.Request.Path.Value.Contains("openai", StringComparison.OrdinalIgnoreCase))
            requestBody = "[FILTERED]";
        logger.LogInformation("[Request] {Method} {Path} | Body: {Body}",
            context.Request.Method, context.Request.Path, requestBody);
        await next(context);
    }
}