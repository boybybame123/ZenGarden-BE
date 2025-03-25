using Microsoft.Extensions.DependencyInjection;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.Hosting;


public class AutoPauseTaskJob(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                await taskService.AutoPauseTasksAsync();
            }
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}