using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UserChallengeService(
    IUserChallengeRepository userChallengeRepository,
    IUnitOfWork unitOfWork,
    ITaskRepository taskRepository) : IUserChallengeService
{
    public async Task UpdateUserChallengeProgressAsync(int userId, int challengeId)
    {
        var userChallenge = await userChallengeRepository.GetUserProgressAsync(userId, challengeId);

        if (userChallenge == null) return;

        var totalTasks = await taskRepository.GetTotalCloneTasksAsync(userId, challengeId);
        var completedTasks = await taskRepository.GetCompletedTasksAsync(userId, challengeId);

        userChallenge.CompletedTasks = completedTasks;
        userChallenge.Progress = totalTasks > 0 ? completedTasks * 100 / totalTasks : 0;
        if (userChallenge.Progress == 100 && userChallenge.Status != UserChallengeStatus.Completed)
            userChallenge.Status = UserChallengeStatus.Completed;
        userChallenge.UpdatedAt = DateTime.UtcNow;
        userChallengeRepository.Update(userChallenge);
        await unitOfWork.CommitAsync();
    }
}