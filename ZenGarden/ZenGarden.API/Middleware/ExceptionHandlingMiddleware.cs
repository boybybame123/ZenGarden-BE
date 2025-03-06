using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ZenGarden.API.Response;

namespace ZenGarden.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred at {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var statusCode = exception switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ValidationException => (int)HttpStatusCode.UnprocessableEntity, 
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            DbException => (int)HttpStatusCode.ServiceUnavailable,
            DbUpdateException => (int)HttpStatusCode.Conflict,
            _ => (int)HttpStatusCode.InternalServerError
        };

        response.StatusCode = statusCode;

        var errorResponse = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = exception.Message,
            Details = statusCode == (int)HttpStatusCode.InternalServerError 
                ? "An unexpected error occurred. Please try again later."
                : exception.InnerException?.Message
        };

        return response.WriteAsync(JsonSerializer.Serialize(errorResponse, JsonOptions));
    }
}
