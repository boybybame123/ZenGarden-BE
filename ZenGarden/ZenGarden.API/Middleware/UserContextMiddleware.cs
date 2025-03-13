namespace ZenGarden.API.Middleware;

public class UserContextMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            context.Items["UserId"] = userId;
        }

        await next(context);
    }
}
