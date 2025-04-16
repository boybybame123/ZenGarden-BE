using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IChallengeService
{
    Task<List<ChallengeDto>> GetAllChallengesAsync();
    Task<ChallengeDto> GetChallengeByIdAsync(int challengeId);
    Task<ChallengeDto> CreateChallengeAsync(int userId, CreateChallengeDto dto);
    Task<bool> JoinChallengeAsync(int userId, int challengeId, JoinChallengeDto joinChallengeDto);
    Task UpdateChallengeAsync(UpdateChallengeDto challenge);
    Task<bool> CancelChallengeAsync(int challengeId, int userId);
    Task<bool> LeaveChallengeAsync(int userId, int challengeId);
    Task<List<UserChallengeRankingDto>> GetChallengeRankingsAsync(int challengeId);
    Task<UserChallengeProgressDto?> GetUserChallengeProgressAsync(int userId, int challengeId);
    Task<TaskDto> CreateTaskForChallengeAsync(int challengeId, CreateTaskDto taskDto);
    Task<string> ChangeStatusChallenge(int userId, int challengeId);
    Task<bool> SelectChallengeWinnerAsync(int organizerId, int challengeId, int winnerUserId);
    Task HandleExpiredChallengesAsync();
    Task NotifyOngoingChallenges();
    Task<List<ChallengeDto>> GetChallengesOngoing();
    Task<List<ChallengeDto>> GetChallengesNotStarted();
}