namespace ZenGarden.Core.Interfaces.IServices;

public interface IUserChallengeService
{
    Task UpdateUserChallengeProgressAsync(int userId, int challengeId);
}