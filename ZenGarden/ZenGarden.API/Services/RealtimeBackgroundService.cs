using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using ZenGarden.API.Hubs;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Services;

public class RealtimeBackgroundService : BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<RealtimeBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory; // Replace ITransactionsService with IServiceScopeFactory

    public RealtimeBackgroundService(
        IHubContext<NotificationHub> hubContext,
        ILogger<RealtimeBackgroundService> logger,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory) // Inject IServiceScopeFactory instead
    {
        _hubContext = hubContext;
        _logger = logger;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delayTime = _configuration.GetValue("RealtimeSettings:DelayInSeconds", 10) * 1000;
        var transactionCheckInterval = TimeSpan.FromMinutes(1); // 15-minute interval
        var lastTransactionCheck = DateTime.UtcNow;

        _logger.LogInformation("RealtimeBackgroundService started at {Time}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Call MarkOldPendingTransactionsAsFailedAsync every 15 minutes
            if (DateTime.UtcNow - lastTransactionCheck >= transactionCheckInterval)
            {
                await HandleOldPendingTransactionsAsync(stoppingToken);
                lastTransactionCheck = DateTime.UtcNow;
            }

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

    private async Task HandleOldPendingTransactionsAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Checking and marking old pending transactions as failed at {Time}", DateTime.UtcNow);

            // Create a new scope to resolve the scoped service
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                // Get the scoped service from the created scope
                var transactionsService = scope.ServiceProvider.GetRequiredService<ITransactionsService>();
                await transactionsService.MarkOldPendingTransactionsAsFailedAsync();
            }

            _logger.LogInformation("Successfully marked old pending transactions as failed at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while marking old pending transactions as failed at {Time}", DateTime.UtcNow);
        }
    }
}