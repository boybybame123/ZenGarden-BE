using Microsoft.AspNetCore.SignalR;
using ZenGarden.API.Hubs;

namespace ZenGarden.API.Services;

public class RealtimeBackgroundService(
    IHubContext<NotificationHub> hubContext,
    ILogger<RealtimeBackgroundService> logger,
    IConfiguration configuration)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delayTime = configuration.GetValue("RealtimeSettings:DelayInSeconds", 10) * 1000;

        logger.LogInformation("RealtimeBackgroundService started at {Time}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SendRealTimeMessage(stoppingToken);
            await Task.Delay(delayTime, stoppingToken); // Add delay to prevent tight loop
        }

        logger.LogInformation("RealtimeBackgroundService stopped at {Time}", DateTime.UtcNow);
    }

    private async Task SendRealTimeMessage(CancellationToken stoppingToken)
    {
        var message = $"Ping real-time from server: {DateTime.Now}";
        try
        {
            logger.LogInformation("Sending real-time message: {Message} at {Time}", message, DateTime.UtcNow);
            await hubContext.Clients.All.SendAsync("ReceiveMessage", message, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while sending real-time message at {Time}", DateTime.UtcNow);
        }
    }
}