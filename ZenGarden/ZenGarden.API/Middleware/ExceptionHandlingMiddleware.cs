using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ZenGarden.API.Response;

namespace ZenGarden.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        int statusCode;

        if (exception.Message.Contains("OpenAI API request failed", StringComparison.OrdinalIgnoreCase))
            statusCode = (int)HttpStatusCode.ServiceUnavailable;
        else
            statusCode = exception switch
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

        var errorResponse = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = statusCode == (int)HttpStatusCode.ServiceUnavailable
                ? "External AI service is unavailable. Please try again later."
                : exception.Message,
            Details = statusCode == (int)HttpStatusCode.InternalServerError
                ? "An unexpected error occurred. Please try again later."
                : exception.InnerException?.Message
        };

        return response.WriteAsync(JsonSerializer.Serialize(errorResponse, JsonOptions));
    }
}