using Moq;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Test.Mocks;

public static class ChallengeRepositoryMock
{
    public static Mock<IChallengeRepository> GetMock()
    {
        var mock = new Mock<IChallengeRepository>();

        var challengeList = new List<Challenge>
        {
            new() { ChallengeId = 1, ChallengeName = "Challenge 1", Description = "Test 1" },
            new() { ChallengeId = 2, ChallengeName = "Challenge 2", Description = "Test 2" }
        };

        mock.Setup(repo => repo.GetChallengeAll()).ReturnsAsync(challengeList);

        mock.Setup(repo => repo.GetByIdChallengeAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => challengeList.FirstOrDefault(c => c.ChallengeId == id));

        mock.Setup(repo => repo.CreateAsync(It.IsAny<Challenge>()))
            .Returns(Task.CompletedTask)
            .Callback<Challenge>(c => challengeList.Add(c));

        mock.Setup(repo => repo.Update(It.IsAny<Challenge>()));

        mock.Setup(repo => repo.RemoveAsync(It.IsAny<Challenge>()))
            .Returns(Task.CompletedTask)
            .Callback<Challenge>(challenge => challengeList.RemoveAll(c => c.ChallengeId == challenge.ChallengeId));

        return mock;
    }
}