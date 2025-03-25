using Microsoft.AspNetCore.SignalR;
using ZenGarden.API.Hubs;
using ZenGarden.Core.Interfaces.IServices;

public class RealtimeBackgroundService : BackgroundService
{
    private readonly ILogger<RealtimeBackgroundService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;

    public RealtimeBackgroundService(ILogger<RealtimeBackgroundService> logger,
                                     IHubContext<NotificationHub> hubContext,
                                     IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = $"🔥 Ping Realtime: {DateTime.Now}";
            _logger.LogInformation(message);

            // Bắn public nếu muốn
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);

            // Gửi 1 noti lưu DB + bắn user
            using (var scope = _scopeFactory.CreateScope())
            {
                var notiService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notiService.PushNotificationAsync(1, "Ping Server", message);
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);

        }
    }
}
