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
                        .GetRequiredService<IChallengeService>();
                await challengeService.HandleExpiredChallengesAsync();
                logger.LogInformation("HandleExpiredChallengesAsync executed successfully at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid();
                logger.LogError(ex,
                    "An unhandled exception occurred in HandleExpiredChallengesJob with Error ID {ErrorId}",
                    errorId);
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("HandleExpiredChallengesJob is stopping.");
                break;
            }
        }
    }
}