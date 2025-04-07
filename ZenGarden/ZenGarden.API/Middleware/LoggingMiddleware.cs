namespace ZenGarden.API.Middleware;

public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    private static async Task<string> ReadRequestBody(HttpRequest request, int maxLength = 1000)
    {
        if (request.ContentLength == null || request.ContentLength == 0 || !request.Body.CanRead)
            return "[Empty Body]";

        if (request.ContentLength > 10 * 1024 * 1024) return "[Content too large to log]";

        if (!request.Body.CanSeek)
        {
            request.EnableBuffering();
            request.Body.Seek(0, SeekOrigin.Begin);
        }

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var bodyText = await reader.ReadToEndAsync();
        request.Body.Seek(0, SeekOrigin.Begin);

        return string.IsNullOrWhiteSpace(bodyText)
            ? "[Empty Body]"
            : bodyText.Length > maxLength
                ? string.Concat(bodyText.AsSpan(0, maxLength), "...")
                : bodyText;
    }

    private static async Task<string> ReadResponseBody(Stream responseBodyStream, int maxLength = 1000)
    {
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var bodyText = await new StreamReader(responseBodyStream, leaveOpen: true).ReadToEndAsync();
        responseBodyStream.Seek(0, SeekOrigin.Begin);

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

        var originalResponseBody = context.Response.Body;
        using var newResponseBody = new MemoryStream();
        context.Response.Body = newResponseBody;
        var startTime = DateTime.UtcNow;

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            logger.LogError(ex, "[Error] {Method} {Path} | Duration: {Duration}ms | Message: {Message}",
                method, path, duration, ex.Message);
            throw;
        }
        finally
        {
            try
            {
                var responseBody = await ReadResponseBody(newResponseBody);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                logger.LogInformation(
                    "[Response] {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | Body: {Body}",
                    method, path, context.Response.StatusCode, duration, responseBody);

                newResponseBody.Seek(0, SeekOrigin.Begin);
                await newResponseBody.CopyToAsync(originalResponseBody);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while logging response for {Method} {Path}", method, path);
            }
            finally
            {
                context.Response.Body = originalResponseBody;
            }
        }
    }
}