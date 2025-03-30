using System.Data.Common;
using System.Net;
using FluentValidation;
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

        var (statusCode, errorResponse) = GetStatusCodeAndErrorResponse(context, exception);
        response.StatusCode = statusCode;

        await response.WriteAsJsonAsync(errorResponse);
    }

    private (int statusCode, object errorResponse) GetStatusCodeAndErrorResponse(HttpContext context,
        Exception exception)
    {
        int statusCode;
        object errorResponse;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                var validationErrors = validationException.Errors
                    .Select(e => new { e.PropertyName, e.ErrorMessage })
                    .ToList();

                logger.LogWarning("Validation failed: {@Errors}", validationErrors);

                errorResponse = new ErrorResponse
                {
                    StatusCode = statusCode,
                    Message = "Validation failed",
                    Details = validationErrors
                };
                break;

            case ArgumentNullException or InvalidOperationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new ErrorResponse("Invalid request", exception.Message);
                break;

            case System.ComponentModel.DataAnnotations.ValidationException:
                statusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = new ErrorResponse("Unprocessable entity", exception.Message);
                break;

            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                logger.LogWarning("Not Found: {Message} - Path: {Path}", exception.Message, context.Request.Path);
                errorResponse = new ErrorResponse("Resource not found", exception.Message);
                break;

            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                logger.LogWarning("Unauthorized access attempt - Path: {Path}", context.Request.Path);
                errorResponse = new ErrorResponse("Unauthorized", exception.Message);
                break;

            case DbException or DbUpdateException:
                statusCode = (int)HttpStatusCode.Conflict;
                logger.LogError(exception, "Database error occurred at {Path}", context.Request.Path);
                errorResponse = new ErrorResponse("Database error", exception.Message);
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                var showDetail = env.IsDevelopment() || config.GetValue("MiddlewareSettings:ShowDetailedErrors", false);
                var errorId = Guid.NewGuid().ToString();
                var traceId = context.TraceIdentifier;

                if (showDetail)
                    logger.LogError(exception, "Error {ErrorId}, TraceId {TraceId} at {Path}. StackTrace: {StackTrace}",
                        errorId, traceId, context.Request.Path, exception.StackTrace);
                else
                    logger.LogError(exception, "Error {ErrorId}, TraceId {TraceId} at {Path}",
                        errorId, traceId, context.Request.Path);

                errorResponse = new ErrorResponse
                {
                    StatusCode = statusCode,
                    Message = "An unexpected error occurred. Please try again later.",
                    Details = showDetail
                        ? new
                        {
                            ErrorId = errorId,
                            TraceId = traceId,
                            ExceptionMessage = exception.Message,
                            exception.StackTrace
                        }
                        : new
                        {
                            ErrorId = errorId,
                            TraceId = traceId
                        }
                };
                break;
        }

        return (statusCode, errorResponse);
    }
}