using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Infrastructure.BackgroundJobs;

public class DailyTaskResetJob(IServiceScopeFactory scopeFactory, ILogger<DailyTaskResetJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            try
            {
                var now = DateTime.UtcNow;
                var nextMidnight = now.Date.AddDays(1);
                var delay = nextMidnight - now;

                await Task.Delay(delay, stoppingToken);

                using var scope = scopeFactory.CreateScope();
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                await taskService.ResetDailyTasksAsync();

                logger.LogInformation("Daily tasks reset executed successfully at {Time}", DateTime.UtcNow);
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("DailyTaskResetJob is stopping.");
                break;
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid();
                logger.LogError(ex, "An unhandled exception occurred in DailyTaskResetJob with Error ID {ErrorId}",
                    errorId);

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    logger.LogInformation("DailyTaskResetJob is stopping after error delay.");
                    break;
                }
            }
    }
}