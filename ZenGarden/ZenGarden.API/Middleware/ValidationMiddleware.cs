using System.Text.Json;

namespace ZenGarden.API.Middleware
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.HasJsonContentType())
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Invalid Content-Type" }));
                return;
            }

            await _next(context);
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