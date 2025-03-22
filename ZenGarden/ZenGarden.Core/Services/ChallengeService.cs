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
    ITaskTypeRepository taskTypeRepository,
    IFocusMethodRepository focusMethodRepository,
    IFocusMethodService focusMethodService,
    IWalletRepository walletRepository,
    IUserRepository userRepository,
    IMapper mapper)
    : IChallengeService
{
    public async Task<ChallengeDto> CreateChallengeAsync(int userId, CreateChallengeDto dto)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (user.Role is { RoleId: 2 })
            {
                var wallet = await walletRepository.GetByUserIdAsync(userId);
                if (wallet == null)
                    throw new InvalidOperationException("Wallet not found.");

                const decimal challengeCost = 100;
                if (wallet.Balance <= challengeCost)
                    throw new InvalidOperationException("Not enough ZenCoin to create challenge.");

                wallet.Balance -= challengeCost;
                wallet.UpdatedAt = DateTime.UtcNow;
                walletRepository.Update(wallet);
            }

            var challenge = mapper.Map<Challenge>(dto);
            challenge.CreatedAt = DateTime.UtcNow;
            await challengeRepository.CreateAsync(challenge);

            var userChallenge = new UserChallenge
            {
                ChallengeId = challenge.ChallengeId,
                UserId = userId,
                Progress = 0,
                ChallengeRole = UserChallengeRole.Organizer,
                Status = UserChallengeStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            await userChallengeRepository.CreateAsync(userChallenge);

            if (dto.Tasks is { Count: > 0 })
            {
                var tasks = dto.Tasks.Select(taskDto => new Tasks
                {
                    TaskName = taskDto.TaskName,
                    TaskDescription = taskDto.TaskDescription,
                    Status = TasksStatus.NotStarted,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await taskRepository.AddRangeAsync(tasks);
                await unitOfWork.CommitAsync();

                var challengeTasks = tasks.Select(task => new ChallengeTask
                {
                    ChallengeId = challenge.ChallengeId,
                    TaskId = task.TaskId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();
                await challengeTaskRepository.AddRangeAsync(challengeTasks);
            }

            await unitOfWork.CommitAsync();
            await transaction.CommitAsync();

            return mapper.Map<ChallengeDto>(challenge);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public async Task<bool> JoinChallengeAsync(int userId, int challengeId, JoinChallengeDto joinChallengeDto)
    {
        var challenge = await challengeRepository.GetByIdAsync(challengeId);
        if (challenge == null)
            throw new KeyNotFoundException("Challenge not found!");

        var existingUserChallenge = await userChallengeRepository.GetUserChallengeAsync(userId, challengeId);
        if (existingUserChallenge != null)
            throw new InvalidOperationException("You have already joined this challenge!");

        var userTree = await userTreeRepository.GetByIdAsync(joinChallengeDto.UserTreeId);
        if (userTree == null || userTree.UserId != userId)
            throw new ArgumentException("Invalid tree selection!");

        if (joinChallengeDto.TaskTypeId.HasValue)
        {
            var existingTaskType = await taskTypeRepository.GetByIdAsync(joinChallengeDto.TaskTypeId.Value);
            if (existingTaskType == null)
                throw new KeyNotFoundException("TaskType not found.");
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var userChallenge = new UserChallenge
            {
                ChallengeId = challengeId,
                UserId = userId,
                ChallengeRole = UserChallengeRole.Participant,
                Status = UserChallengeStatus.InProgress,
                JoinedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await userChallengeRepository.CreateAsync(userChallenge);

            var challengeTasks = await challengeTaskRepository.GetTasksByChallengeIdAsync(challengeId);
            var newTasks = new List<Tasks>();

            foreach (var ct in challengeTasks)
            {
                if (ct.Tasks == null) continue;

                FocusMethodDto selectedMethod;
                if (ct.Tasks.FocusMethodId.HasValue)
                {
                    selectedMethod = (await focusMethodRepository.GetDtoByIdAsync(ct.Tasks.FocusMethodId.Value))!;
                }
                else
                {
                    selectedMethod = await focusMethodService.SuggestFocusMethodAsync(new SuggestFocusMethodDto
                    {
                        TaskName = ct.Tasks.TaskName,
                        TaskDescription = ct.Tasks.TaskDescription,
                        TotalDuration = ct.Tasks.TotalDuration,
                        StartDate = ct.Tasks.StartDate ?? challenge.StartDate,
                        EndDate = ct.Tasks.EndDate ?? challenge.EndDate
                    });

                    if (selectedMethod == null)
                        throw new InvalidOperationException("No valid focus method found.");
                }

                var newTask = new Tasks
                {
                    TaskTypeId = joinChallengeDto.TaskTypeId ?? 0,
                    UserTreeId = joinChallengeDto.UserTreeId,
                    FocusMethodId = selectedMethod.FocusMethodId,
                    TaskName = ct.Tasks.TaskName,
                    TaskDescription = ct.Tasks.TaskDescription,
                    TotalDuration = ct.Tasks.TotalDuration,
                    WorkDuration = selectedMethod.DefaultDuration ?? 25,
                    BreakTime = selectedMethod.DefaultBreak ?? 5,
                    Status = TasksStatus.NotStarted,
                    CreatedAt = DateTime.UtcNow
                };

                newTasks.Add(newTask);
            }

            await taskRepository.AddRangeAsync(newTasks);
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
        var challenges = await challengeRepository.GetAllAsync();
        return mapper.Map<List<ChallengeDto>>(challenges);
    }

    public async Task<ChallengeDto> GetChallengeByIdAsync(int challengeId)
    {
        var challenge = await challengeRepository.GetByIdAsync(challengeId);
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
        if (userChallenge == null)
            throw new KeyNotFoundException("You are not part of this challenge!");

        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var userTasks = await taskRepository.GetTasksByUserChallengeAsync(userId, challengeId);

            if (userTasks.Count > 0)
            {
                userTasks.ForEach(task =>
                {
                    task.Status = TasksStatus.Canceled;
                    task.UpdatedAt = DateTime.UtcNow;
                });

                await taskRepository.UpdateRangeAsync(userTasks);
            }

            userChallenge.Status = UserChallengeStatus.Canceled;
            userChallenge.UpdatedAt = DateTime.UtcNow;
            userChallengeRepository.Update(userChallenge);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
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

            var userChallenges = await userChallengeRepository.GetAllUsersInChallengeAsync(challengeId);
            foreach (var uc in userChallenges)
            {
                uc.Status = UserChallengeStatus.Canceled;
                uc.UpdatedAt = DateTime.UtcNow;
            }

            await userChallengeRepository.UpdateRangeAsync(userChallenges);

            var tasks = await taskRepository.GetTasksByChallengeIdAsync(challengeId);
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
}