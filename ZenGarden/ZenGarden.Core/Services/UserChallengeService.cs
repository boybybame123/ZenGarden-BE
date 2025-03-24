using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Core.Services;

public class UserChallengeService(IUserChallengeRepository userChallengeRepository) : IUserChallengeService
{
    public async Task UpdateUserChallengeProgressAsync(int userId, int challengeId)
    {
        await userChallengeRepository.UpdateProgressAsync(userId, challengeId);
    }
}