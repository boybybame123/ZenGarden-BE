using System.Data.Common;
using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ZenGarden.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment env,
    IConfiguration config)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        if (exception is ValidationException validationException)
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
            System.ComponentModel.DataAnnotations.ValidationException => (int)HttpStatusCode.UnprocessableEntity,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            DbException or DbUpdateException => (int)HttpStatusCode.Conflict,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var showDetail = env.IsDevelopment() || config.GetValue("MiddlewareSettings:ShowDetailedErrors", false);

        var errorId = Guid.NewGuid().ToString();
        var traceId = context.TraceIdentifier;

        logger.LogError(exception, "Error {ErrorId}, TraceId {TraceId} at {Path}", errorId, traceId,
            context.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "An error occurred",
            Detail = showDetail ? exception.Message : "An unexpected error occurred. Please try again later.",
            Instance = context.Request.Path
        };
        problemDetails.Extensions.Add("errorId", errorId);
        problemDetails.Extensions.Add("traceId", traceId);

        response.StatusCode = statusCode;
        await response.WriteAsJsonAsync(problemDetails);
    }
}