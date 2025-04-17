using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Infrastructure.BackgroundJobs;

public class WeeklyTaskResetService : BackgroundService
{
    private readonly ILogger<WeeklyTaskResetService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public WeeklyTaskResetService(IServiceProvider serviceProvider, ILogger<WeeklyTaskResetService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Tính toán thời gian đến thứ Hai tiếp theo
            var now = DateTime.Now;
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0 && now.Hour >= 0) // Nếu hôm nay là thứ Hai và đã qua 0h
                daysUntilMonday = 7; // Đợi đến thứ Hai tuần sau

            var nextMonday = now.Date.AddDays(daysUntilMonday).AddHours(0);
            var delay = nextMonday - now;

            _logger.LogInformation($"Next task priority reset scheduled for {nextMonday}");

            // Đợi đến thứ Hai tiếp theo
            await Task.Delay(delay, stoppingToken);

            // Thực hiện reset
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                await taskService.WeeklyTaskPriorityResetAsync();
                _logger.LogInformation("Weekly task priority reset completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during weekly task priority reset");
            }
        }
    }
}