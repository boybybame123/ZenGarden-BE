using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class ChallengeService(IChallengeRepository challengeRepository, IUnitOfWork unitOfWork, IMapper mapper)
    : IChallengeService
{
    public async Task CreateChallengeAsync(ChallengeDto Challenge)
    {
        var check = await challengeRepository.GetByIdAsync(Challenge.ChallengeId);
        if (check == null)
        {
            var Challenges = mapper.Map<Challenge>(Challenge);
            await challengeRepository.CreateAsync(Challenges);
            await unitOfWork.CommitAsync();
        }
        else
        {
            throw new Exception("Challenge already exists");
        }
    }

    public Task DeleteChallengeAsync(int ChallengeId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Challenge>> GetAllChallengeAsync()
    {
        var challenges = await challengeRepository.GetCallengeAll();
        return challenges;
    }

    public async Task<Challenge?> GetChallengeByIdAsync(int ChallengeId)
    {
        var Challenges = await challengeRepository.GetByIdChallengeAsync(ChallengeId);
        return Challenges;
    }

    public async Task UpdateChallengeAsync(ChallengeDto Challenge)
    {
        var existingChallenge = await challengeRepository.GetByIdAsync(Challenge.ChallengeId);
        if (!string.IsNullOrEmpty(Challenge.ChallengeName)) existingChallenge.ChallengeName = Challenge.ChallengeName;

        if (!string.IsNullOrEmpty(Challenge.Description)) existingChallenge.Description = Challenge.Description;

        if (Challenge.ChallengeTypeId != 0) existingChallenge.ChallengeTypeId = Challenge.ChallengeTypeId;

        if (Challenge.XpReward != 0) existingChallenge.XpReward = Challenge.XpReward;

        if (Challenge.status != existingChallenge.status) existingChallenge.status = Challenge.status;

        challengeRepository.Update(existingChallenge);
        await unitOfWork.CommitAsync();
    }
}