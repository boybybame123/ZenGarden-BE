using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ZenGarden.API.Middleware;

public class ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
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

                    var errorResponse = new
                    {
                        Status = context.Response.StatusCode,
                        Message = "Invalid Content-Type",
                        Detail = "Content-Type must be application/json."
                    };

                    await context.Response.WriteAsJsonAsync(errorResponse);
                    return;
                }

            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation failed: {Errors}", ex.Errors);

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var problemDetails = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = "Validation failed",
                Detail = "One or more validation errors occurred.",
                Instance = context.Request.Path,
                Extensions =
                {
                    ["errors"] = ex.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                }
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred.");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var problemDetails = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
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