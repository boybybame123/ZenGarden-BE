using System.Text.Json;
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
    INotificationService notificationService,
    IUserChallengeRepository userChallengeRepository,
    ITaskRepository taskRepository,
    IChallengeTaskRepository challengeTaskRepository,
    IUserTreeRepository userTreeRepository,
    IWalletRepository walletRepository,
    IUserRepository userRepository,
    ITaskService taskService,
    IRedisService redisService,
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

        if (dto.MaxParticipants is > 100)
            throw new InvalidOperationException("Maximum participants cannot exceed 100.");

        var challenge = mapper.Map<Challenge>(dto);
        challenge.CreatedAt = DateTime.UtcNow;
        
        // Set status based on user role
        challenge.Status = user.Role?.RoleId is 1 or 3 ? ChallengeStatus.Active : ChallengeStatus.Pending;
        
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
        await ClearChallengeCachesAsync(challenge.ChallengeId);

        // Send notification based on status
        if (challenge.Status == ChallengeStatus.Active)
        {
            await notificationService.PushNotificationAsync(
                userId,
                "Challenge Created",
                $"Your challenge '{challenge.ChallengeName}' has been created and activated. Join now!"
            );
        }
        else
        {
            await notificationService.PushNotificationAsync(
                userId,
                "Challenge Created",
                $"Your challenge '{challenge.ChallengeName}' has been created and is pending approval."
            );
        }

        return mapper.Map<ChallengeDto>(challenge);
    }

    public async Task<TaskDto> CreateTaskForChallengeAsync(int challengeId, CreateTaskDto taskDto)
    {
        if (await challengeRepository.GetByIdAsync(challengeId) is null)
            throw new KeyNotFoundException("Challenge not found.");

        taskDto.TaskTypeId = 4; // Assuming 4 is the TaskTypeId for Challenge tasks
        var createdTask = await taskService.CreateTaskWithSuggestedMethodAsync(taskDto);

        var challengeTask = new ChallengeTask
        {
            ChallengeId = challengeId,
            TaskId = createdTask.TaskId,
            CreatedAt = DateTime.UtcNow
        };

        await challengeTaskRepository.CreateAsync(challengeTask);
        await unitOfWork.CommitAsync();
        await ClearChallengeCachesAsync(challengeId);
        return createdTask;
    }

    public async Task<bool> JoinChallengeAsync(int userId, int challengeId, JoinChallengeDto joinChallengeDto)
    {
        var challenge = await challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null) return false;

        var participantCount = await userChallengeRepository.CountParticipantsAsync(challengeId);
        if (challenge.MaxParticipants.HasValue && participantCount >= challenge.MaxParticipants.Value)
            throw new InvalidOperationException("Challenge has reached the maximum number of participants.");

        await ValidateJoinChallenge(challenge, userId, joinChallengeDto);

        var strategy = unitOfWork.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(
            new object(),
            async (_, _, _) =>
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

                if (taskList.Count != 0) await taskRepository.AddRangeAsync(taskList);

                await unitOfWork.CommitAsync();
                await NotifyOngoingChallenges();
                await ClearChallengeCachesAsync(challengeId);
                return true;
            },
            null,
            CancellationToken.None
        );
    }

    public async Task<List<ChallengeDto>> GetAllChallengesAsync()
    {
        const string cacheKey = "all_challenges";
        var cached = await redisService.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cached))
        {
            var cachedChallenges = JsonSerializer.Deserialize<List<ChallengeDto>>(cached);
            if (cachedChallenges != null) return cachedChallenges;
        }

        var challenges = await challengeRepository.GetChallengeAll();
        var result = mapper.Map<List<ChallengeDto>>(challenges);

        foreach (var challengeDto in result)
            challengeDto.CurrentParticipants =
                await userChallengeRepository.CountParticipantsAsync(challengeDto.ChallengeId);

        var serialized = JsonSerializer.Serialize(result);
        await redisService.SetStringAsync(cacheKey, serialized, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<ChallengeDto> GetChallengeByIdAsync(int challengeId)
    {
        var cacheKey = $"challenge_{challengeId}";
        var cached = await redisService.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cached))
        {
            var cachedChallenge = JsonSerializer.Deserialize<ChallengeDto>(cached);
            if (cachedChallenge != null) return cachedChallenge;
        }

        var challenge = await challengeRepository.GetByIdChallengeAsync(challengeId);
        var result = mapper.Map<ChallengeDto>(challenge);

        result.CurrentParticipants =
            await userChallengeRepository.CountParticipantsAsync(result.ChallengeId);

        var serialized = JsonSerializer.Serialize(result);
        await redisService.SetStringAsync(cacheKey, serialized, TimeSpan.FromMinutes(5));

        return result;
    }


    public async Task UpdateChallengeAsync(int challengeId, UpdateChallengeDto challengeDto)
    {
        var existingChallenge = await challengeRepository.GetByIdAsync(challengeId);
        if (existingChallenge == null)
            throw new KeyNotFoundException("Challenge not found.");

        var now = DateTime.UtcNow;

        if (existingChallenge.StartDate <= now && existingChallenge.EndDate >= now)
            throw new InvalidOperationException("Challenge is ongoing and cannot be updated.");

        if (!string.IsNullOrEmpty(challengeDto.ChallengeName))
            existingChallenge.ChallengeName = challengeDto.ChallengeName;

        if (!string.IsNullOrEmpty(challengeDto.Description))
            existingChallenge.Description = challengeDto.Description;

        if (challengeDto.Reward.HasValue)
            existingChallenge.Reward = challengeDto.Reward.Value;

        if (challengeDto.StartDate.HasValue)
            existingChallenge.StartDate = challengeDto.StartDate.Value;

        if (challengeDto.EndDate.HasValue)
            existingChallenge.EndDate = challengeDto.EndDate.Value;

        if (challengeDto.MaxParticipants.HasValue)
            existingChallenge.MaxParticipants = challengeDto.MaxParticipants.Value;

        if (challengeDto.ChallengeTypeId.HasValue)
            existingChallenge.ChallengeTypeId = challengeDto.ChallengeTypeId.Value;

        existingChallenge.UpdatedAt = DateTime.UtcNow;

        challengeRepository.Update(existingChallenge);
        await ClearChallengeCachesAsync(challengeId);
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
        await ClearChallengeCachesAsync(challengeId);
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
            await ClearChallengeCachesAsync(challengeId);

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
        var cacheKey = $"challenge_rankings_{challengeId}";
        var cached = await redisService.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cached))
        {
            var cachedList = JsonSerializer.Deserialize<List<UserChallengeRankingDto>>(cached);
            if (cachedList != null) return cachedList;
        }

        var userChallenges = await userChallengeRepository.GetRankedUserChallengesAsync(challengeId);

        var dtoList = userChallenges.Select(uc => new UserChallengeRankingDto
        {
            UserId = uc.UserId,
            UserName = uc.User?.UserName ?? "Unknown",
            Progress = uc.Progress,
            CompletedTasks = uc.CompletedTasks,
            IsWinner = uc.IsWinner
        }).ToList();

        var serialized = JsonSerializer.Serialize(dtoList);
        await redisService.SetStringAsync(cacheKey, serialized, TimeSpan.FromMinutes(5));

        return dtoList;
    }

    public async Task<UserChallengeProgressDto?> GetUserChallengeProgressAsync(int userId, int challengeId)
    {
        var cacheKey = $"user_challenge_progress_{userId}_{challengeId}";
        var cached = await redisService.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cached))
        {
            var progress = JsonSerializer.Deserialize<UserChallengeProgressDto>(cached);
            if (progress != null) return progress;
        }

        var userChallenge = await userChallengeRepository.GetUserProgressAsync(userId, challengeId);
        var result = userChallenge == null ? null : mapper.Map<UserChallengeProgressDto>(userChallenge);

        if (result == null) return result;
        var serialized = JsonSerializer.Serialize(result);
        await redisService.SetStringAsync(cacheKey, serialized, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<string> ChangeStatusChallenge(int userId, int challengeId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var challenge = await challengeRepository.GetByIdAsync(challengeId);
        if (challenge == null)
            throw new KeyNotFoundException("Challenge not found.");

        if (challenge.Status != ChallengeStatus.Pending) 
            return "Challenge status is already Active or Canceled";

        // Get the challenge creator's user challenge record
        var creatorChallenge = await userChallengeRepository.GetUserChallengeAsync(userId, challengeId);
        if (creatorChallenge == null)
            throw new KeyNotFoundException("Challenge creator not found.");

        // Check if there are any participants
        var participants = await userChallengeRepository.GetAllUsersInChallengeAsync(challengeId);
        var hasParticipants = participants.Any(p => p.ChallengeRole == UserChallengeRole.Participant);

        // If creator is role 2 (player), only role 1 and 3 can approve
        if (creatorChallenge.User?.Role?.RoleId == 2)
        {
            if (user.Role?.RoleId is not (1 or 3))
                throw new UnauthorizedAccessException("Only administrators can approve challenges created by players.");

            // If challenge is rejected, refund the creator
            if (!hasParticipants)
            {
                var creatorWallet = await walletRepository.GetByUserIdAsync(creatorChallenge.UserId);
                if (creatorWallet != null)
                {
                    creatorWallet.Balance += challenge.Reward;
                    creatorWallet.UpdatedAt = DateTime.UtcNow;
                    walletRepository.Update(creatorWallet);
                }

                challenge.Status = ChallengeStatus.Canceled;
                challenge.UpdatedAt = DateTime.UtcNow;
                challengeRepository.Update(challenge);

                await notificationService.PushNotificationAsync(
                    creatorChallenge.UserId,
                    "Challenge Rejected",
                    $"Your challenge '{challenge.ChallengeName}' has been rejected and the reward has been refunded to your wallet."
                );

                await unitOfWork.CommitAsync();
                await ClearChallengeCachesAsync(challengeId);
                return "Challenge has been rejected and reward refunded";
            }
        }

        await unitOfWork.CommitAsync();
        await ClearChallengeCachesAsync(challengeId);
        return "Challenge status changed to Active";
    }

    public async Task<bool> SelectChallengeWinnersAsync(int organizerId, int challengeId, SelectWinnerDto dto)
    {
        var challenge = await challengeRepository.GetByIdAsync(challengeId)
                        ?? throw new KeyNotFoundException("Challenge not found.");

        var organizer = await userChallengeRepository.GetUserChallengeAsync(organizerId, challengeId);
        if (organizer is not { ChallengeRole: UserChallengeRole.Organizer })
            throw new UnauthorizedAccessException("Only the organizer can select the winners.");

        if (dto.Winners.Count == 0)
            throw new InvalidOperationException("No winners provided.");

        var allUserChallenges = await userChallengeRepository.GetAllUsersInChallengeAsync(challengeId);
        var winnerIds = dto.Winners.Select(w => w.UserId).ToHashSet();
        var completedUserIds = await userChallengeRepository.GetCompletedUserIdsAsync(challengeId);

        if (!winnerIds.IsSubsetOf(completedUserIds))
            throw new InvalidOperationException("One or more winners have not completed the challenge.");

        foreach (var uc in allUserChallenges)
        {
            uc.IsWinner = winnerIds.Contains(uc.UserId);
            uc.UpdatedAt = DateTime.UtcNow;
        }

        await userChallengeRepository.UpdateRangeAsync(allUserChallenges);

        var totalReward = challenge.Reward;
        var winnerCount = dto.Winners.Count;
        var rewardPerWinner = totalReward / winnerCount;
        var remainder = totalReward % winnerCount;

        for (var i = 0; i < dto.Winners.Count; i++)
        {
            var winner = dto.Winners[i];
            var wallet = await walletRepository.GetByUserIdAsync(winner.UserId)
                         ?? throw new InvalidOperationException($"Wallet not found for user {winner.UserId}");

            var reward = rewardPerWinner + (i < remainder ? 1 : 0);

            wallet.Balance += reward;
            wallet.UpdatedAt = DateTime.UtcNow;
            walletRepository.Update(wallet);

            await notificationService.PushNotificationAsync(
                winner.UserId,
                "Challenge Winner",
                $"Congratulations! You've won the challenge and received {reward} ZenCoin.\n" +
                $"🏆 Reason: {winner.Reason}"
            );
        }

        await unitOfWork.CommitAsync();
        await ClearChallengeCachesAsync(challengeId);

        return true;
    }


    public async Task HandleExpiredChallengesAsync()
    {
        var now = DateTime.UtcNow;

        var challenges = await challengeRepository.GetExpiredInProgressChallengesAsync(now);

        foreach (var challenge in challenges)
        {
            var userChallenges = await userChallengeRepository.GetAllUsersInChallengeAsync(challenge.ChallengeId);

            foreach (var uc in userChallenges)
                if (uc.ChallengeRole == UserChallengeRole.Organizer)
                {
                    uc.UpdatedAt = DateTime.UtcNow;
                }
                else if (uc is { Status: UserChallengeStatus.InProgress, Progress: < 100 })
                {
                    uc.Status = UserChallengeStatus.Failed;
                    uc.UpdatedAt = DateTime.UtcNow;
                }

            challenge.Status = ChallengeStatus.Expired;
            challenge.UpdatedAt = DateTime.UtcNow;

            await userChallengeRepository.UpdateRangeAsync(userChallenges);
            challengeRepository.Update(challenge);
            await ClearChallengeCachesAsync(challenge.ChallengeId);
        }

        await unitOfWork.CommitAsync();
    }


    public async Task NotifyOngoingChallenges()
    {
        var ongoingChallenges = await challengeRepository.GetOngoingChallengesAsync();
        var dtoList = new List<ChallengeDto>();

        foreach (var challenge in ongoingChallenges)
        {
            var dto = mapper.Map<ChallengeDto>(challenge);

            dto.CurrentParticipants = await userChallengeRepository.CountParticipantsAsync(challenge.ChallengeId);

            dtoList.Add(dto);
        }

        var serialized = JsonSerializer.Serialize(dtoList);
        await redisService.SetStringAsync("active_challenges", serialized, TimeSpan.FromMinutes(5));
    }


    public async Task<List<ChallengeDto>> GetChallengesOngoing()
    {
        var cached = await redisService.GetStringAsync("active_challenges");

        if (!string.IsNullOrEmpty(cached))
        {
            var dtoList = JsonSerializer.Deserialize<List<ChallengeDto>>(cached);
            if (dtoList != null) return dtoList;
        }

        await NotifyOngoingChallenges();

        cached = await redisService.GetStringAsync("active_challenges");
        var freshData = JsonSerializer.Deserialize<List<ChallengeDto>>(cached);
        return freshData ?? [];
    }

    public async Task<List<ChallengeDto>> GetChallengesNotStarted()
    {
        const string cacheKey = "not_started_challenges";
        var cached = await redisService.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cached))
        {
            var dtoList = JsonSerializer.Deserialize<List<ChallengeDto>>(cached);
            if (dtoList != null) return dtoList;
        }

        var notStartedChallenges = await challengeRepository.GetChallengesNotStartedAsync();
        var mapped = mapper.Map<List<ChallengeDto>>(notStartedChallenges);

        var serialized = JsonSerializer.Serialize(mapped);
        await redisService.SetStringAsync(cacheKey, serialized, TimeSpan.FromMinutes(5));

        return mapped;
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

    private async Task ClearChallengeCachesAsync(int challengeId, int? userId = null)
    {
        await redisService.RemoveAsync($"challenge_{challengeId}");
        await redisService.RemoveAsync("all_challenges");
        await redisService.RemoveAsync("active_challenges");
        await redisService.RemoveAsync("not_started_challenges");
        await redisService.RemoveAsync($"challenge_rankings_{challengeId}");
        if (userId.HasValue) await redisService.RemoveAsync($"user_challenge_progress_{userId.Value}_{challengeId}");
    }
}