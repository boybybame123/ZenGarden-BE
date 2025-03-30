using Moq;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Test.Mocks;

public static class ChallengeRepositoryMock
{
    public static Mock<IChallengeRepository> GetMock()
    {
        var mock = new Mock<IChallengeRepository>();

        var challengeTypes = new List<ChallengeType>
        {
            new() { ChallengeTypeId = 1, ChallengeTypeName = "Type A" },
            new() { ChallengeTypeId = 2, ChallengeTypeName = "Type B" }
        };

        var challengeList = new List<Challenge>
        {
            new()
            {
                ChallengeId = 1,
                ChallengeName = "Challenge 1",
                ChallengeTypeId = 1, // Liên kết với ChallengeTypeId
                ChallengeType = challengeTypes.First(c => c.ChallengeTypeId == 1), // Gán object ChallengeType
                Status = ChallengeStatus.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                Reward = 100,
                UserChallenges = new List<UserChallenge>
                {
                    new() { UserId = 101, ChallengeId = 1 },
                    new() { UserId = 102, ChallengeId = 1 }
                },
                ChallengeTasks = new List<ChallengeTask>
                {
                    new()
                    {
                        TaskId = 201,
                        Tasks = new Tasks
                        {
                            TaskId = 201,
                            TaskName = "Task A",
                            Status = TasksStatus.NotStarted
                        }
                    }
                }
            },
            new()
            {
                ChallengeId = 2,
                ChallengeName = "Challenge 2",
                ChallengeTypeId = 2,
                ChallengeType = challengeTypes.First(c => c.ChallengeTypeId == 2),
                Status = ChallengeStatus.Pending,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(5),
                Reward = 50,
                UserChallenges = new List<UserChallenge>
                {
                    new() { UserId = 103, ChallengeId = 2 }
                },
                ChallengeTasks = new List<ChallengeTask>
                {
                    new()
                    {
                        TaskId = 202,
                        Tasks = new Tasks
                        {
                            TaskId = 202,
                            TaskName = "Task B",
                            Status = TasksStatus.Completed
                        }
                    }
                }
            }
        };

        // Mock GetChallengeAll
        mock.Setup(repo => repo.GetChallengeAll()).ReturnsAsync(challengeList);

        // Mock GetByIdChallengeAsync
        mock.Setup(repo => repo.GetByIdChallengeAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => challengeList.FirstOrDefault(c => c.ChallengeId == id));

        return mock;
    }
}