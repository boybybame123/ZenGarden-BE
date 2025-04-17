using Microsoft.AspNetCore.SignalR;
using ZenGarden.API.Hubs;

namespace ZenGarden.API.Services;

public class RealtimeBackgroundService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<RealtimeBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public RealtimeBackgroundService(
        IHubContext<NotificationHub> hubContext,
        ILogger<RealtimeBackgroundService> logger,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory)
    {
        _hubContext = hubContext;
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delayTime = _configuration.GetValue("RealtimeSettings:DelayInSeconds", 10) * 1000;

        _logger.LogInformation("RealtimeBackgroundService started at {Time}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SendRealTimeMessage(stoppingToken);
            await Task.Delay(delayTime, stoppingToken); // Add delay to prevent tight loop
        }

        _logger.LogInformation("RealtimeBackgroundService stopped at {Time}", DateTime.UtcNow);
    }

    private async Task SendRealTimeMessage(CancellationToken stoppingToken)
    {
        var message = $"Ping real-time from server: {DateTime.Now}";
        try
        {
            _logger.LogInformation("Sending real-time message: {Message} at {Time}", message, DateTime.UtcNow);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending real-time message at {Time}", DateTime.UtcNow);
        }
    }
}