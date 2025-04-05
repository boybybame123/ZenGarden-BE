using System.Data.Common;
using System.Diagnostics;
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
        var sw = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        finally
        {
            sw.Stop();
            if (sw.ElapsedMilliseconds > 1000)
            {
                logger.LogWarning("Long running request: {Method} {Path} took {Duration}ms",
                    context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds);
            }
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
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
            
            case HttpRequestException httpRequestEx:
                statusCode = (int)HttpStatusCode.BadGateway;
                logger.LogError(httpRequestEx, "HTTP request error at {Path}", context.Request.Path);
                errorResponse = new ErrorResponse(statusCode,"Bad Gateway", httpRequestEx.Message);
                break;
            
            case TaskCanceledException taskEx:
                statusCode = 408; 
                logger.LogWarning(taskEx,"Request timed out - Path: {Path}", context.Request.Path);
                errorResponse = new ErrorResponse(statusCode,"Request timeout", taskEx.Message);
                break;
            
            case  System.Text.Json.JsonException jsonEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new ErrorResponse(statusCode,"Invalid JSON format", jsonEx.Message);
                break;
            
            case OperationCanceledException:
                statusCode = 499; 
                logger.LogWarning("Request canceled or timed out - Path: {Path}", context.Request.Path);
                errorResponse = new ErrorResponse(statusCode,"Operation canceled", "The request was canceled or timed out");
                break;
            
            case OutOfMemoryException:
                statusCode = (int)HttpStatusCode.ServiceUnavailable;
                logger.LogCritical(exception, "Server resource exhaustion at {Path}", context.Request.Path);
                errorResponse = new ErrorResponse(statusCode,"Service temporarily unavailable", "The server is currently unable to handle the request due to temporary overloading");
                break;
            
            case BusinessRuleException businessEx:  
                statusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = new ErrorResponse(statusCode,"Business rule violation", businessEx.Message);
                break;

            case ArgumentNullException or InvalidOperationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new ErrorResponse(statusCode,"Invalid request", exception.Message);
                break;

            case System.ComponentModel.DataAnnotations.ValidationException:
                statusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = new ErrorResponse(statusCode,"Unprocessable entity", exception.Message);
                break;

            case KeyNotFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                logger.LogWarning("Not Found: {Message} - Path: {Path}", exception.Message, context.Request.Path);
                errorResponse = new ErrorResponse(statusCode,"Resource not found", exception.Message);
                break;

            case UnauthorizedAccessException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                logger.LogWarning("Unauthorized access attempt - Path: {Path}", context.Request.Path);
                errorResponse = new ErrorResponse(statusCode,"Unauthorized", exception.Message);
                break;

            case DbException or DbUpdateException or MySqlConnector.MySqlException:
                statusCode = (int)HttpStatusCode.Conflict;
                logger.LogError(exception, "Database error occurred at {Path}", context.Request.Path);
                errorResponse = new ErrorResponse(statusCode,"Database connection error", "The system could not connect to the database. Please try again later.");
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
public abstract class BusinessRuleException(string message) : Exception(message);