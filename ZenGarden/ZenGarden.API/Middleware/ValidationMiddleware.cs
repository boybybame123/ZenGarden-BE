using System.Text.Json;

namespace ZenGarden.API.Middleware
{
    public class ValidationMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.HasJsonContentType())
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Invalid Content-Type" }));
                return;
            }

            await next(context);
        }
    }

    public static class HttpRequestExtensions
    {
        public static bool HasJsonContentType(this HttpRequest request)
        {
            return request.ContentType != null &&
                   request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
        }
    }
}