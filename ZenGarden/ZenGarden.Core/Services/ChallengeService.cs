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
            XpReward = dto.XpReward,
            Status = ChallengeStatus.Pending
        };

        await challengeRepository.CreateAsync(challenge);
        await unitOfWork.CommitAsync();

        var userChallenge = new UserChallenge
        {
            ChallengeId = challenge.ChallengeId,
            UserId = userId,
            ChallengeRole = UserChallengeRole.Organizer
        };

        await userChallengeRepository.CreateAsync(userChallenge);
        await unitOfWork.CommitAsync();

        if (dto.Tasks == null || dto.Tasks.Count == 0) return mapper.Map<ChallengeDto>(challenge);
        foreach (var task in dto.Tasks.Select(taskDto => new Tasks
                 {
                     TaskName = taskDto.TaskName,
                     TaskDescription = taskDto.TaskDescription,
                     Status = TasksStatus.NotStarted,
                     CreatedAt = DateTime.UtcNow
                 }))
        {
            await taskRepository.CreateAsync(task);
            await unitOfWork.CommitAsync();

            var challengeTask = new ChallengeTask
            {
                ChallengeId = challenge.ChallengeId,
                TaskId = task.TaskId
            };

            await challengeTaskRepository.CreateAsync(challengeTask);
        }

        await unitOfWork.CommitAsync();

        return mapper.Map<ChallengeDto>(challenge);
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

        if (challenge.XpReward != 0)
            if (existingChallenge != null)
                existingChallenge.XpReward = challenge.XpReward;

        if (existingChallenge != null && challenge.Status != existingChallenge.Status) existingChallenge.Status = challenge.Status;

        if (existingChallenge != null) challengeRepository.Update(existingChallenge);
        await unitOfWork.CommitAsync();
    }
}