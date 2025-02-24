using System.Diagnostics;

namespace ZenGarden.API.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        private static async Task<string> ReadRequestBody(HttpRequest request, int maxLength = 1000)
        {
            request.EnableBuffering();
            var bodyStream = new StreamReader(request.Body);
            var bodyText = await bodyStream.ReadToEndAsync();
            request.Body.Position = 0;
            return bodyText.Length > maxLength ? bodyText.Substring(0, maxLength) + "..." : bodyText;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestBody = await ReadRequestBody(context.Request);
            _logger.LogInformation("[Request] {Method} {Path} | Body: {Body}", 
                context.Request.Method, context.Request.Path, requestBody);

            await _next(context);
        }
    }
}