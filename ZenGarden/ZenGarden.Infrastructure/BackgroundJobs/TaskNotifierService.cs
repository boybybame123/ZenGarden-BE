using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Infrastructure.BackgroundJobs;

public class TaskNotifierService(
    IServiceScopeFactory scopeFactory,
    ILogger<TaskNotifierService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("⏰ Task Notifier Service is starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNotificationsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error occurred while processing task notifications");
            }

            // Chạy mỗi phút
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessNotificationsAsync()
    {
        var currentTime = DateTime.UtcNow;
        logger.LogDebug("🔄 Checking for tasks to notify at {CurrentTime}", currentTime);

        using var scope = scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();


        // Lấy các task cần thông báo tại thời điểm hiện tại
        var tasksToNotify = await taskService.GetTasksToNotifyAsync(currentTime);

        foreach (var task in tasksToNotify)
        {
            if (task.UserTree?.UserId == null)
                continue;

            var userId = task.UserTree.UserId.Value;
            var isStartDateNotification = task.StartDate.HasValue &&
                                          task.StartDate.Value.Date == currentTime.Date &&
                                          task.StartDate.Value.Hour == currentTime.Hour &&
                                          task.StartDate.Value.Minute == currentTime.Minute;

            var isEndDateReminder = task.EndDate.HasValue &&
                                    task.Status != TasksStatus.Completed;

            if (isStartDateNotification)
            {
                await notificationService.PushNotificationAsync(
                    userId,
                    "Task Ready to Start",
                    $"It's time to start your task: {task.TaskName}");

                logger.LogInformation("📅 Sent start notification for task {TaskId} to user {UserId}",
                    task.TaskId, userId);
            }
            else if (isEndDateReminder)
            {
                var notificationTitle = "Task Deadline Reminder";
                var notificationContent = $"Don't forget to complete your task: {task.TaskName}";

                if (task.EndDate != null && task.EndDate.Value.Date == currentTime.AddDays(1).Date)
                {
                    notificationContent = $"Your task '{task.TaskName}' is due tomorrow!";
                }
                else if (task.EndDate != null &&
                         task.EndDate.Value.Date == currentTime.Date &&
                         (task.EndDate.Value - currentTime).TotalMinutes <= 5)
                {
                    notificationTitle = "Urgent Deadline!";
                    notificationContent = $"Your task '{task.TaskName}' needs to be completed in the next 5 minutes!";
                }

                await notificationService.PushNotificationAsync(
                    userId,
                    notificationTitle,
                    notificationContent);

                logger.LogInformation("⏰ Sent deadline notification for task {TaskId} to user {UserId}",
                    task.TaskId, userId);
            }
        }
    }
}