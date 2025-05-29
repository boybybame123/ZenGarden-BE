using System.Net;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Middleware;

public class ValidationMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            await next(context);
            return;
        }

        if (context.Request.ContentType != null &&
            context.Request.ContentType.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (!(HttpMethods.IsGet(context.Request.Method) ||
              HttpMethods.IsHead(context.Request.Method) ||
              HttpMethods.IsDelete(context.Request.Method) ||
              HttpMethods.IsOptions(context.Request.Method)))
            if (context.Request.ContentLength is > 0 &&
                !context.Request.HasJsonContentType())
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                var errorResponse = new ErrorResponse
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "Invalid Content-Type",
                    Details = "Content-Type must be application/json."
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
                return;
            }

        await next(context);
    }
}

public static class HttpRequestExtensions
{
    public static bool HasJsonContentType(this HttpRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.ContentType) &&
               request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}