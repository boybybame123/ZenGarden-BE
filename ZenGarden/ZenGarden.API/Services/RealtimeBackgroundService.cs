using Microsoft.AspNetCore.SignalR;
using ZenGarden.API.Hubs;

public class RealtimeBackgroundService : BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<RealtimeBackgroundService> _logger;

    public RealtimeBackgroundService(IHubContext<NotificationHub> hubContext, ILogger<RealtimeBackgroundService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = $"Ping real-time từ server: {DateTime.Now}";
            _logger.LogInformation("Push realtime: " + message);

            // ✅ Tự động bắn về tất cả client SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

            await Task.Delay(1000, stoppingToken); // Mỗi 10s tự bắn 1 lần
        }
    }
}
