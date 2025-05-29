using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZenGarden.Core.Hubs;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.API.Services;

public class RealtimeBackgroundService(
    IHubContext<NotificationHub> hubContext,
    ILogger<RealtimeBackgroundService> logger,
    IConfiguration configuration,
    IServiceScopeFactory serviceScopeFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delayTime = configuration.GetValue("RealtimeSettings:DelayInSeconds", 10) * 1000;
        var transactionCheckInterval = TimeSpan.FromMinutes(15); // 15-minute interval
        var lastTransactionCheck = DateTime.UtcNow;

        logger.LogInformation("RealtimeBackgroundService started at {Time}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Call MarkOldPendingTransactionsAsFailedAsync every 15 minutes
            if (DateTime.UtcNow - lastTransactionCheck >= transactionCheckInterval)
            {
                await HandleOldPendingTransactionsAsync();
                lastTransactionCheck = DateTime.UtcNow;
            }

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

    private async Task HandleOldPendingTransactionsAsync()
    {
        try
        {
            logger.LogInformation("Checking and marking old pending transactions as failed at {Time}", DateTime.UtcNow);

            // Create a new scope to resolve the scoped service
            using (var scope = serviceScopeFactory.CreateScope())
            {
                // Get the scoped service from the created scope
                var transactionsService = scope.ServiceProvider.GetRequiredService<ITransactionsService>();
                await transactionsService.MarkOldPendingTransactionsAsFailedAsync();
            }

            logger.LogInformation("Successfully marked old pending transactions as failed at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while marking old pending transactions as failed at {Time}", DateTime.UtcNow);
        }
    }
}