using System.Net;
using System.Text.Json;
using ZenGarden.API.Response;

namespace ZenGarden.API.Middleware
{
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
                logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var statusCode = exception switch
            {
                ArgumentNullException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, 
                System.Data.Common.DbException => (int)HttpStatusCode.ServiceUnavailable, 
                Microsoft.EntityFrameworkCore.DbUpdateException => (int)HttpStatusCode.Conflict, 
                _ => (int)HttpStatusCode.InternalServerError
            };

            response.StatusCode = statusCode;

            var errorResponse = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = exception.Message,
                Details = exception.InnerException?.Message
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            return response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
        }
    }
}
