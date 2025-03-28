using System.Data.Common;
using System.Net;
using FluentValidation;
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
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        int statusCode;
        object errorResponse;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new
                {
                    StatusCode = statusCode,
                    Message = "Validation failed",
                    Errors = validationException.Errors
                        .Select(e => new { e.PropertyName, e.ErrorMessage })
                        .ToList()
                };
                break;

            case ArgumentNullException:
            case InvalidOperationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new ErrorResponse("Invalid request", exception.Message);
                break;

            case System.ComponentModel.DataAnnotations.ValidationException:
                statusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = new ErrorResponse("Unprocessable entity", exception.Message);
                break;

            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                errorResponse = new ErrorResponse("Resource not found", exception.Message);
                break;

            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse = new ErrorResponse("Unauthorized", exception.Message);
                break;

            case DbException or DbUpdateException:
                statusCode = (int)HttpStatusCode.Conflict;
                errorResponse = new ErrorResponse("Database error", exception.Message);
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                var showDetail = env.IsDevelopment() || config.GetValue("MiddlewareSettings:ShowDetailedErrors", false);
                var errorId = Guid.NewGuid().ToString();
                var traceId = context.TraceIdentifier;

                logger.LogError(exception, "Error {ErrorId}, TraceId {TraceId} at {Path}",
                    errorId, traceId, context.Request.Path);

                errorResponse = new ProblemDetails
                {
                    Status = statusCode,
                    Title = "An error occurred",
                    Detail = showDetail ? exception.Message : "An unexpected error occurred. Please try again later.",
                    Instance = context.Request.Path,
                    Extensions = { { "errorId", errorId }, { "traceId", traceId } }
                };
                break;
        }

        response.StatusCode = statusCode;
        await response.WriteAsJsonAsync(errorResponse);
    }
}