namespace ZenGarden.API.Middleware;

public class JwtMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        await next(context);
    }
}