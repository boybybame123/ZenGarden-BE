using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Infrastructure.BackgroundJobs;

public class AutoPauseTaskJob(IServiceScopeFactory scopeFactory, ILogger<AutoPauseTaskJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                await taskService.AutoPauseTasksAsync();
                logger.LogInformation("AutoPauseTasksAsync executed successfully at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid();
                logger.LogError(ex, "An unhandled exception occurred in AutoPauseTaskJob with Error ID {ErrorId}",
                    errorId);
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("AutoPauseTaskJob is stopping.");
                break;
            }
        }
    }
}