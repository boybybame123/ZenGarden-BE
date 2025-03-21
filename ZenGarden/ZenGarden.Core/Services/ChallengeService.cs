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
    IMapper mapper)
    : IChallengeService
{
    public async Task<ChallengeDto> CreateChallengeAsync(int userId, CreateChallengeDto dto)
    {
        var challenge = new Challenge
        {
            ChallengeTypeId = dto.ChallengeTypeId,
            ChallengeName = dto.ChallengeName,
            Description = dto.Description,
            Reward = dto.Reward,
            Status = ChallengeStatus.Pending,
            StartDate = dto.StartDate ?? DateTime.UtcNow,
            EndDate = dto.EndDate ?? DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

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

        if (dto.Tasks == null || dto.Tasks.Count == 0) 
            return mapper.Map<ChallengeDto>(challenge);

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
        await unitOfWork.CommitAsync();

        return mapper.Map<ChallengeDto>(challenge);
    }
    
    public async Task<bool> JoinChallengeAsync(int userId, int challengeId, int userTreeId, int? taskTypeId)
{
    var challenge = await challengeRepository.GetByIdAsync(challengeId);
    if (challenge == null)
        throw new KeyNotFoundException("Challenge not found!");

    var existingUserChallenge = await userChallengeRepository.GetUserChallengeAsync(userId, challengeId);
    if (existingUserChallenge != null)
        throw new InvalidOperationException("You have already joined this challenge!");

    var userTree = await userTreeRepository.GetByIdAsync(userTreeId);
    if (userTree == null || userTree.UserId != userId)
        throw new ArgumentException("Invalid tree selection!");

    if (taskTypeId.HasValue)
    {
        var existingTaskType = await taskTypeRepository.GetByIdAsync(taskTypeId.Value);
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
                TaskTypeId = taskTypeId ?? 0,
                UserTreeId = userTreeId,
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
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        Console.WriteLine($"Error joining challenge: {ex.Message}");
        throw;
    }
}

    
    public Task DeleteChallengeAsync(int challengeId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Challenge>> GetAllChallengeAsync()
    {
        var challenges = await challengeRepository.GetCallengeAll();
        return challenges;
    }

    public async Task<Challenge> GetChallengeByIdAsync(int challengeId)
    {
        var challenges = await challengeRepository.GetByIdChallengeAsync(challengeId);
        return challenges;
    }

    public async Task UpdateChallengeAsync(ChallengeDto challenge)
    {
        var existingChallenge = await challengeRepository.GetByIdAsync(challenge.ChallengeId);
        if (!string.IsNullOrEmpty(challenge.ChallengeName))
            if (existingChallenge != null)
                existingChallenge.ChallengeName = challenge.ChallengeName;

        if (!string.IsNullOrEmpty(challenge.Description))
            if (existingChallenge != null)
                existingChallenge.Description = challenge.Description;

        if (challenge.ChallengeTypeId != 0)
            if (existingChallenge != null)
                existingChallenge.ChallengeTypeId = challenge.ChallengeTypeId;

        if (challenge.Reward != 0)
            if (existingChallenge != null)
                existingChallenge.Reward = challenge.Reward;

        if (existingChallenge != null && challenge.Status != existingChallenge.Status) existingChallenge.Status = challenge.Status;

        if (existingChallenge != null) challengeRepository.Update(existingChallenge);
        await unitOfWork.CommitAsync();
    }
}