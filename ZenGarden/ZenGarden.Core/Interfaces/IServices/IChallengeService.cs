using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IChallengeService
{
    Task<List<ChallengeDto>> GetAllChallengesAsync();
    Task<ChallengeDto> GetChallengeByIdAsync(int challengeId);
    Task<ChallengeDto> CreateChallengeAsync(int userId, CreateChallengeDto dto);
    Task<bool> JoinChallengeAsync(int userId, int challengeId, JoinChallengeDto joinChallengeDto);
    Task UpdateChallengeAsync(int challengeId, UpdateChallengeDto challengeDto);
    Task<bool> CancelChallengeAsync(int challengeId, int userId);
    Task<bool> LeaveChallengeAsync(int userId, int challengeId);
    Task<List<UserChallengeRankingDto>> GetChallengeRankingsAsync(int challengeId);
    Task<UserChallengeProgressDto?> GetUserChallengeProgressAsync(int userId, int challengeId);
    Task<TaskDto> CreateTaskForChallengeAsync(int challengeId, CreateTaskDto taskDto);
    Task<string> ChangeStatusChallenge(int userId, int challengeId);
    Task<string> RejectChallengeAsync(int userId, int challengeId);
    Task<bool> SelectChallengeWinnersAsync(int organizerId, int challengeId, SelectWinnerDto dto);
    Task HandleExpiredChallengesAsync();
    Task NotifyOngoingChallenges();
    Task<List<ChallengeDto>> GetChallengesOngoing();
    Task<List<ChallengeDto>> GetChallengesNotStarted();
}