using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class ChallengeService(
    IChallengeRepository challengeRepository,
    IUnitOfWork unitOfWork,
    IUserChallengeRepository userChallengeRepository,
    ITaskRepository taskRepository,
    IChallengeTaskRepository challengeTaskRepository,
    IUserTreeRepository userTreeRepository,
    IWalletRepository walletRepository,
    IUserRepository userRepository,
    ITaskService taskService,
    IMapper mapper)
    : IChallengeService
{
    public async Task<ChallengeDto> CreateChallengeAsync(int userId, CreateChallengeDto dto)
    {
        var user = await userRepository.GetByIdAsync(userId)
                   ?? throw new KeyNotFoundException("User not found.");

        if (user.Role is { RoleId: 2 })
        {
            var wallet = await walletRepository.GetByUserIdAsync(userId)
                         ?? throw new InvalidOperationException("Wallet not found.");

            if (wallet.Balance < dto.Reward)
                throw new InvalidOperationException("Not enough ZenCoin to create challenge.");

            wallet.Balance -= dto.Reward;
            wallet.UpdatedAt = DateTime.UtcNow;
            walletRepository.Update(wallet);
        }

        var challenge = mapper.Map<Challenge>(dto);
        challenge.CreatedAt = DateTime.UtcNow;
        await challengeRepository.CreateAsync(challenge);
        await unitOfWork.CommitAsync();

        var userChallenge = new UserChallenge
        {
            ChallengeId = challenge.ChallengeId,
            UserId = userId,
            ChallengeRole = UserChallengeRole.Organizer,
            Status = UserChallengeStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        await userChallengeRepository.CreateAsync(userChallenge);
        await unitOfWork.CommitAsync();

        return mapper.Map<ChallengeDto>(challenge);
    }

    public async Task<TaskDto> CreateTaskForChallengeAsync(int challengeId, CreateTaskDto taskDto)
    {
        if (await challengeRepository.GetByIdAsync(challengeId) is null)
            throw new KeyNotFoundException("Challenge not found.");

        var createdTask = await taskService.CreateTaskWithSuggestedMethodAsync(taskDto);

        var challengeTask = new ChallengeTask
        {
            ChallengeId = challengeId,
            TaskId = createdTask.TaskId,
            CreatedAt = DateTime.UtcNow
        };

        await challengeTaskRepository.CreateAsync(challengeTask);
        await unitOfWork.CommitAsync();
        return createdTask;
    }

    public async Task<bool> JoinChallengeAsync(int userId, int challengeId, JoinChallengeDto joinChallengeDto)
    {
        var challenge = await challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null) return false;

        await ValidateJoinChallenge(challenge, userId, joinChallengeDto);

        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            await userChallengeRepository.CreateAsync(new UserChallenge
            {
                ChallengeId = challengeId,
                UserId = userId,
                ChallengeRole = UserChallengeRole.Participant,
                Status = UserChallengeStatus.InProgress,
                JoinedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            var challengeTasks = await challengeTaskRepository.GetTasksByChallengeIdAsync(challengeId);
            var taskList = new List<Tasks>();

            foreach (var ct in challengeTasks)
            {
                if (ct.Tasks == null) continue;
                var newTask = new Tasks
                {
                    CloneFromTaskId = ct.TaskId,
                    TaskTypeId = ct.Tasks.TaskTypeId,
                    UserTreeId = joinChallengeDto.UserTreeId,
                    TaskName = ct.Tasks.TaskName,
                    TaskDescription = ct.Tasks.TaskDescription,
                    TotalDuration = ct.Tasks.TotalDuration,
                    StartDate = ct.Tasks.StartDate ?? challenge.StartDate,
                    EndDate = ct.Tasks.EndDate ?? challenge.EndDate,
                    WorkDuration = ct.Tasks.WorkDuration,
                    BreakTime = ct.Tasks.BreakTime,
                    FocusMethodId = ct.Tasks.FocusMethodId
                };
                
                taskList.Add(newTask);
            }

            if (taskList.Count != 0)
            {
                await taskRepository.AddRangeAsync(taskList);
            }

            await unitOfWork.CommitAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public async Task<List<ChallengeDto>> GetAllChallengesAsync()
    {
        var challenges = await challengeRepository.GetChallengeAll();
        return mapper.Map<List<ChallengeDto>>(challenges);
    }

    public async Task<ChallengeDto> GetChallengeByIdAsync(int challengeId)
    {
        var challenge = await challengeRepository.GetByIdChallengeAsync(challengeId);
        return mapper.Map<ChallengeDto>(challenge);
    }

    public async Task UpdateChallengeAsync(UpdateChallengeDto challenge)
    {
        var existingChallenge = await challengeRepository.GetByIdAsync(challenge.ChallengeId);
        if (existingChallenge == null)
            throw new KeyNotFoundException("Challenge not found.");

        mapper.Map(challenge, existingChallenge);

        challengeRepository.Update(existingChallenge);
        await unitOfWork.CommitAsync();
    }

    public async Task<bool> LeaveChallengeAsync(int userId, int challengeId)
    {
        var userChallenge = await userChallengeRepository.GetUserChallengeAsync(userId, challengeId);
        if (userChallenge == null) return false;

        if (userChallenge.ChallengeRole == UserChallengeRole.Organizer)
            throw new InvalidOperationException("Organizer cannot leave the challenge.");
        var userTasks = await taskRepository.GetClonedTasksByUserChallengeAsync(userId, challengeId);
        await using var transaction = await unitOfWork.BeginTransactionAsync(); 

        switch (userTasks.Count)
        {
            case 0:
                return false;
            case > 0:
            {
                foreach (var task in userTasks)
                {
                    task.Status = TasksStatus.Canceled;
                    task.UpdatedAt = DateTime.UtcNow;
                }

                await taskRepository.UpdateRangeAsync(userTasks);
                break;
            }
        }

        userChallenge.Status = UserChallengeStatus.Canceled;
            userChallenge.UpdatedAt = DateTime.UtcNow;
            userChallengeRepository.Update(userChallenge);
            await unitOfWork.CommitAsync();
            await transaction.CommitAsync();
            return true;
    }

    public async Task<bool> CancelChallengeAsync(int challengeId, int userId)
    {
        var challenge = await challengeRepository.GetByIdAsync(challengeId);
        if (challenge == null)
            throw new KeyNotFoundException("Challenge not found.");

        var userChallenge = await userChallengeRepository.GetUserChallengeAsync(userId, challengeId);
        if (userChallenge is not { ChallengeRole: UserChallengeRole.Organizer })
            throw new UnauthorizedAccessException("Only the organizer can cancel this challenge.");

        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            challenge.Status = ChallengeStatus.Canceled;
            challenge.UpdatedAt = DateTime.UtcNow;
            challengeRepository.Update(challenge);
            await unitOfWork.CommitAsync();

            var userChallenges = await userChallengeRepository.GetAllUsersInChallengeAsync(challengeId);
            foreach (var uc in userChallenges)
            {
                uc.Status = UserChallengeStatus.Canceled;
                uc.UpdatedAt = DateTime.UtcNow;
            }

            await userChallengeRepository.UpdateRangeAsync(userChallenges);
            await unitOfWork.CommitAsync();

            var tasks = await taskRepository.GetAllTasksByChallengeIdAsync(challengeId);
            foreach (var task in tasks)
            {
                task.Status = TasksStatus.Canceled;
                task.UpdatedAt = DateTime.UtcNow;
            }

            await taskRepository.UpdateRangeAsync(tasks);

            await unitOfWork.CommitAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<UserChallengeRankingDto>> GetChallengeRankingsAsync(int challengeId)
    {
        var userChallenges = await userChallengeRepository.GetRankedUserChallengesAsync(challengeId);

        return userChallenges.Select(uc => new UserChallengeRankingDto
        {
            UserId = uc.UserId,
            UserName = uc.User?.UserName ?? "Unknown",
            Progress = uc.Progress,
            CompletedTasks = uc.CompletedTasks
        }).ToList();
    }

    public async Task<UserChallengeProgressDto?> GetUserChallengeProgressAsync(int userId, int challengeId)
    {
        var userChallenge = await userChallengeRepository.GetUserProgressAsync(userId, challengeId);
        return userChallenge == null ? null : mapper.Map<UserChallengeProgressDto>(userChallenge);
    }

    private async Task ValidateJoinChallenge(Challenge challenge, int userId, JoinChallengeDto joinChallengeDto)
    {
        if (challenge.Status == ChallengeStatus.Canceled)
            throw new InvalidOperationException("This challenge has been canceled and cannot be joined.");

        if (await userChallengeRepository.GetUserChallengeAsync(userId, challenge.ChallengeId) != null)
            throw new InvalidOperationException("You have already joined this challenge!");

        var userTree = await userTreeRepository.GetByIdAsync(joinChallengeDto.UserTreeId);
        if (userTree == null || userTree.UserId != userId)
            throw new ArgumentException("Invalid tree selection!");
    }
}