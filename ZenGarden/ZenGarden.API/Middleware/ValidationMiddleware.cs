using System.Net;
using System.Text.Json;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Middleware;

public class ValidationMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        if (context.Request.ContentType != null &&
            context.Request.ContentType.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (!HttpMethods.IsGet(context.Request.Method) &&
            !HttpMethods.IsHead(context.Request.Method) &&
            !HttpMethods.IsDelete(context.Request.Method) &&
            !HttpMethods.IsOptions(context.Request.Method))
        {
            if (context.Request.ContentLength > 0 && !context.Request.HasJsonContentType())
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                var errorResponse = new ErrorResponse("Invalid Content-Type", "Content-Type must be application/json.");
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                return;
            }
        }

        await next(context);
    }
}

public static class HttpRequestExtensions
{
    public static bool HasJsonContentType(this HttpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ContentType))
            return false;

        return request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}