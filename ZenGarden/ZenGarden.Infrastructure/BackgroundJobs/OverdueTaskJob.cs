using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Infrastructure.BackgroundJobs;

public class OverdueTaskJob(IServiceScopeFactory scopeFactory, ILogger<OverdueTaskJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OverdueTaskJob started at {StartTime}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

                logger.LogInformation("Executing UpdateOverdueTasksAsync at {Time}", DateTime.UtcNow);
                await taskService.UpdateOverdueTasksAsync();
                logger.LogInformation("Successfully executed UpdateOverdueTasksAsync at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid().ToString();
                logger.LogError(ex,
                    "An error occurred during UpdateOverdueTasksAsync. Error ID: {ErrorId}, Timestamp: {Timestamp}",
                    errorId, DateTime.UtcNow);
            }

            try
            {
                // Delay 1 giờ, nhưng vẫn có thể hủy giữa chừng nếu `stoppingToken` được kích hoạt.
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Xử lý khi job bị hủy (thường xảy ra khi ứng dụng tắt)
                logger.LogInformation("OverdueTaskJob is stopping at {StopTime}", DateTime.UtcNow);
                break;
            }
        }

        logger.LogInformation("OverdueTaskJob stopped at {EndTime}", DateTime.UtcNow);
    }
}