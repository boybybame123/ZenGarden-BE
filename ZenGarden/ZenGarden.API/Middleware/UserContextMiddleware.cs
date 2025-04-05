namespace ZenGarden.API.Middleware
{
    public class UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
    {
        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                logger.LogInformation("User is authenticated.");

                foreach (var claim in context.User.Claims)
                {
                    logger.LogDebug("Claim: {ClaimType} = {ClaimValue}", claim.Type, claim.Value);
                }

                var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                if (userIdClaim != null && int.TryParse(userIdClaim, out var userId))
                {
                    context.Items["UserId"] = userId;
                    logger.LogInformation("Parsed UserId: {userId}", userId);
                }
                else
                {
                    logger.LogWarning("Failed to parse UserId from claim 'userId': {userIdClaim}", userIdClaim);
                }
            }
            else
            {
                logger.LogInformation("User is NOT authenticated.");
            }

            await next(context);
        }
    }

    public static class HttpContextExtensions
    {
        public static int? GetUserId(this HttpContext context)
        {
            return context.Items["UserId"] is int id ? id : null;
        }
    }
}