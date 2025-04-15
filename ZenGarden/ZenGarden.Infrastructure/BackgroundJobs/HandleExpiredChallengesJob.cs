using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Infrastructure.BackgroundJobs;

public class HandleExpiredChallengesJob(IServiceScopeFactory scopeFactory, ILogger<HandleExpiredChallengesJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var challengeService =
                    scope.ServiceProvider
                        .GetRequiredService<IChallengeService>(); // Thay IChallengeService bằng service tương ứng
                await challengeService.HandleExpiredChallengesAsync(); // Gọi phương thức xử lý hết hạn challenge
                logger.LogInformation("HandleExpiredChallengesAsync executed successfully at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid();
                logger.LogError(ex,
                    "An unhandled exception occurred in HandleExpiredChallengesJob with Error ID {ErrorId}",
                    errorId);
            }

            // Delay 1 phút trước khi chạy lại
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1),
                    stoppingToken); // Bạn có thể điều chỉnh thời gian delay theo nhu cầu
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("HandleExpiredChallengesJob is stopping.");
                break;
            }
        }
    }
}