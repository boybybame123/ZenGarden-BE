using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using ZenGarden.API.Hubs;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Core.Services;
using ZenGarden.Domain.DTOs;

namespace ZenGarden.API.Services;

public class RealtimeBackgroundService : BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<RealtimeBackgroundService> _logger;

    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    public RealtimeBackgroundService(
        IHubContext<NotificationHub> hubContext,
        ILogger<RealtimeBackgroundService> logger,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory)
    {
        _hubContext = hubContext;
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delayTime = _configuration.GetValue("RealtimeSettings:DelayInSeconds", 10) * 1000;

        _logger.LogInformation("RealtimeBackgroundService started at {Time}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SendRealTimeMessage(stoppingToken);
            //await NotifyOngoingChallenges(stoppingToken);
            await DelayExecution(delayTime, stoppingToken);
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

    private async Task NotifyOngoingChallenges(CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var challengeService = scope.ServiceProvider.GetRequiredService<IChallengeService>();
                await challengeService.NotifyOngoingChallenges();
                await BroadcastOngoingChallenges(challengeService);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while notifying ongoing challenges at {Time}", DateTime.UtcNow);
        }
    }

    private async Task BroadcastOngoingChallenges(IChallengeService challengeService)
    {
        var ongoingChallenges = await challengeService.GetAllChallengesAsync();

        if (ongoingChallenges == null || !ongoingChallenges.Any())
        {
            var noChallengesMessage = "No ongoing challenges at the moment.";
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", noChallengesMessage);
            _logger.LogInformation(noChallengesMessage + " Time: {Time}", DateTime.UtcNow);
            return;
        }

        var detailedList = ongoingChallenges.Select(c =>
            $"[{c.ChallengeId}] {c.ChallengeName} | Reward: {c.Reward} | From: {c.StartDate:yyyy-MM-dd} To: {c.EndDate:yyyy-MM-dd}");

        var formattedMessage = "**Ongoing Challenges:**\n" + string.Join("\n", detailedList);

        // Gửi tới tất cả client
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", formattedMessage);

        // Ghi log chi tiết
        _logger.LogInformation("Broadcasted ongoing challenges:\n{Challenges}\nTime: {Time}",
            string.Join("\n", detailedList), DateTime.UtcNow);


    }

    private async Task DelayExecution(int delayTime, CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(delayTime, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("RealtimeBackgroundService is stopping at {Time}", DateTime.UtcNow);
        }
    }

    public async Task NotifyUserOnLogin(string userId)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var redisService = scope.ServiceProvider.GetRequiredService<IRedisService>();
                var challengeService = scope.ServiceProvider.GetRequiredService<IChallengeService>();


                await SendActiveChallengesToUser(userId, redisService,challengeService);
                await SendRealTimeMessage(CancellationToken.None); // Send real-time message to the user
              
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while notifying user {UserId} of active challenges at {Time}", userId, DateTime.UtcNow);
        }
    }

    private async Task SendActiveChallengesToUser(string userId, IRedisService redisService, IChallengeService challengeService)
    {
        await challengeService.NotifyOngoingChallenges();
        var activeChallengesJson = await redisService.GetStringAsync("active_challenges");

        if (!string.IsNullOrEmpty(activeChallengesJson))
        {
            try
            {
                var challenges = JsonSerializer.Deserialize<List<ChallengeDto>>(activeChallengesJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (challenges is not { Count: > 0 })
                {
                    await _hubContext.Clients.User(userId)
                        .SendAsync("ReceiveMessage", "Hiện tại không có thử thách nào đang diễn ra.");
                    return;
                }

                var formattedList = challenges.Select(c =>
                    $"[{c.ChallengeId}] {c.ChallengeName} - 🎯 {c.Description} | 🎁 {c.Reward} pts | 🕒 {c.StartDate:MM/dd} → {c.EndDate:MM/dd}");

                var message = "**Các thử thách đang diễn ra:**\n" + string.Join("\n", formattedList);

                await _hubContext.Clients.User(userId).SendAsync("ReceiveMessage", message);
                _logger.LogInformation("Sent formatted active challenges to user {UserId} at {Time}", userId, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing active challenges for user {UserId} at {Time}", userId, DateTime.UtcNow);
            }
        }
    }

}
