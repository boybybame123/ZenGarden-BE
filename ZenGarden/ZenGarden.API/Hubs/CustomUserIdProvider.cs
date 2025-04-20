using Microsoft.AspNetCore.SignalR;

namespace ZenGarden.API.Hubs;

public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var httpContext = connection.GetHttpContext();
        var userId = httpContext?.Request.Query["userId"].ToString();
        return userId;
    }
}