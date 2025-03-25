﻿using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public class CustomUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // Lấy UserId từ Claim (nếu có)
        var userId = connection.User?.FindFirst("UserId")?.Value;

        // Hoặc fallback về Identity.Name
        return userId ?? connection.User?.Identity?.Name;
    }
}
