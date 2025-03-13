using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.DTOs.Response;

namespace ZenGarden.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment env,
    IConfiguration config)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly bool _enableOpenAiCheck = config.GetValue<bool>("MiddlewareSettings:EnableOpenAICheck");

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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
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

        if (_enableOpenAiCheck &&
            exception.Message.Contains("OpenAI API request failed", StringComparison.OrdinalIgnoreCase))
            statusCode = (int)HttpStatusCode.ServiceUnavailable;

        var errorResponse = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = exception.Message,
            Details = env.IsDevelopment()
                ? exception.StackTrace
                : "An unexpected error occurred. Please try again later."
        };

        response.StatusCode = statusCode;
        await response.WriteAsync(JsonSerializer.Serialize(errorResponse, JsonOptions));
    }
}