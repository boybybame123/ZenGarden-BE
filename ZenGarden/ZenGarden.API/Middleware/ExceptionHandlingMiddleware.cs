using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZenGarden.Domain.DTOs;

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
        
        if (exception is FluentValidation.ValidationException validationException)
        {
            var validationErrors = validationException.Errors
                .Select(e => new { e.PropertyName, e.ErrorMessage })
                .ToList();

            response.StatusCode = (int)HttpStatusCode.BadRequest;
            await response.WriteAsJsonAsync(new
            {
                response.StatusCode,
                Message = "Validation failed",
                Errors = validationErrors
            });
            return;
        }

        var statusCode = exception switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ValidationException => (int)HttpStatusCode.UnprocessableEntity,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            DbException or DbUpdateException => (int)HttpStatusCode.Conflict,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var errorId = Guid.NewGuid().ToString();
        logger.LogError(exception, "Error {ErrorId} at {Path}", errorId, context.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "An error occurred",
            Detail = env.IsDevelopment() ? exception.Message : "An unexpected error occurred. Please try again later.",
            Instance = context.Request.Path,
            Extensions = { ["errorId"] = errorId }
        };

        response.StatusCode = statusCode;
        await response.WriteAsJsonAsync(problemDetails);
    }

}