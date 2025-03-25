using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Infrastructure.BackgroundJobs;

public class OverdueTaskJob(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                await taskService.UpdateOverdueTasksAsync();
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}