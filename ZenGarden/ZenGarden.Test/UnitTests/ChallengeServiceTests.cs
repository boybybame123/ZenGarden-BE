/*
using Moq;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Core.Services;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace ZenGarden.Test.UnitTests;

public class ChallengeServiceTests
{
    private readonly IChallengeService _challengeService;
    private readonly Mock<IChallengeRepository> _challengeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserChallengeRepository> _userChallengeRepositoryMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IChallengeTaskRepository> _challengeTaskRepositoryMock;
    private readonly Mock<IUserTreeRepository> _userTreeRepositoryMock;
    private readonly Mock<ITaskTypeRepository> _taskTypeRepositoryMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ChallengeService>> _loggerMock;

    public ChallengeServiceTests()
    {
        _challengeRepositoryMock = new Mock<IChallengeRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userChallengeRepositoryMock = new Mock<IUserChallengeRepository>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _challengeTaskRepositoryMock = new Mock<IChallengeTaskRepository>();
        _userTreeRepositoryMock = new Mock<IUserTreeRepository>();
        _taskTypeRepositoryMock = new Mock<ITaskTypeRepository>();
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _taskServiceMock = new Mock<ITaskService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ChallengeService>>();

        _challengeService = new ChallengeService(
            _challengeRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _userChallengeRepositoryMock.Object,
            _taskRepositoryMock.Object,
            _challengeTaskRepositoryMock.Object,
            _userTreeRepositoryMock.Object,
            _taskTypeRepositoryMock.Object,
            _walletRepositoryMock.Object,
            _userRepositoryMock.Object,
            _taskServiceMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task GetChallengeAll_ShouldReturnAllChallenges()
    {
        var challenges = new List<Challenge>
        {
            new() { ChallengeId = 1, ChallengeName = "Challenge 1" },
            new() { ChallengeId = 2, ChallengeName = "Challenge 2" }
        };

        _challengeRepositoryMock.Setup(repo => repo.GetChallengeAll()).ReturnsAsync(challenges);

        var result = await _challengeService.GetAllChallengesAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Theory]
    [InlineData(1, "Challenge 1")]
    [InlineData(2, "Challenge 2")]
    public async Task GetByIdChallengeAsync_ShouldReturnCorrectChallenge(int id, string expectedName)
    {
        var challenge = new Challenge { ChallengeId = id, ChallengeName = expectedName };
        _challengeRepositoryMock.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(challenge);

        var result = await _challengeService.GetChallengeByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(expectedName, result.ChallengeName);
    }

    [Fact]
    public async Task GetByIdChallengeAsync_WithInvalidId_ShouldReturnNull()
    {
        _challengeRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Challenge?)null);

        var result = await _challengeService.GetChallengeByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateChallenge_ShouldAddNewChallenge()
    {
        var newChallenge = new Challenge { ChallengeId = 3, ChallengeName = "New Challenge" };

        _challengeRepositoryMock.Setup(repo => repo.CreateAsync(newChallenge)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(uow => uow.CommitAsync()).Returns(Task.CompletedTask);

        await _challengeService.CreateChallengeAsync(newChallenge);

        _challengeRepositoryMock.Verify(repo => repo.CreateAsync(newChallenge), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateChallenge_ShouldModifyExistingChallenge()
    {
        var challenge = new Challenge { ChallengeId = 1, ChallengeName = "Updated Challenge" };

        _challengeRepositoryMock.Setup(repo => repo.Update(challenge)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(uow => uow.CommitAsync()).Returns(Task.CompletedTask);

        await _challengeService.UpdateChallengeAsync(challenge);

        _challengeRepositoryMock.Verify(repo => repo.Update(challenge), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteChallenge_ShouldRemoveChallenge()
    {
        _challengeRepositoryMock.Setup(repo => repo.DeleteAsync(1)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _challengeService.DeleteChallenge(1);

        _challengeRepositoryMock.Verify(repo => repo.DeleteAsync(1), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }
}
*/

