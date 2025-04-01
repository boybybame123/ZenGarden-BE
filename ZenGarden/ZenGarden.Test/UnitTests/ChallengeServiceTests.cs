using AutoMapper;
using Moq;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Core.Services;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;
using ZenGarden.Test.Mocks;

namespace ZenGarden.Test.UnitTests;

public class ChallengeServiceTests
{
    private readonly Mock<IChallengeRepository> _challengeRepositoryMock;
    private readonly ChallengeService _challengeService;
    private readonly Mock<IChallengeTaskRepository> _challengeTaskRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<ITaskTypeRepository> _taskTypeRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserChallengeRepository> _userChallengeRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserTreeRepository> _userTreeRepositoryMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;

    public ChallengeServiceTests()
    {
        _challengeRepositoryMock = ChallengeRepositoryMock.GetMock();
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
    public async Task CreateChallengeAsync_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Users?)null);
        var dto = new CreateChallengeDto { Reward = 100 };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _challengeService.CreateChallengeAsync(1, dto));
    }

    [Fact]
    public async Task CreateChallengeAsync_ShouldThrowException_WhenNotEnoughZenCoin()
    {
        // Arrange
        var user = new Users { UserId = 1, Role = new Roles { RoleId = 2 } };
        var wallet = new Wallet { Balance = 50 }; // Không đủ ZenCoin
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
        _walletRepositoryMock.Setup(w => w.GetByUserIdAsync(It.IsAny<int>())).ReturnsAsync(wallet);
        var dto = new CreateChallengeDto { Reward = 100 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _challengeService.CreateChallengeAsync(1, dto));
    }

    [Fact]
    public async Task CreateChallengeAsync_ShouldSucceed_WhenValidUserAndWallet()
    {
        // Arrange
        var user = new Users { UserId = 1, Role = new Roles { RoleId = 2 } };
        var wallet = new Wallet { Balance = 200 };
        var dto = new CreateChallengeDto { Reward = 100 };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
        _walletRepositoryMock.Setup(w => w.GetByUserIdAsync(It.IsAny<int>())).ReturnsAsync(wallet);

        _mapperMock.Setup(m => m.Map<Challenge>(It.IsAny<CreateChallengeDto>()))
            .Returns(new Challenge { ChallengeId = 1, CreatedAt = DateTime.UtcNow });

        _challengeRepositoryMock.Setup(c => c.CreateAsync(It.IsAny<Challenge>()))
            .Callback<Challenge>(c => c.ChallengeId = 1)
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(m => m.Map<ChallengeDto>(It.IsAny<Challenge>()))
            .Returns(new ChallengeDto { ChallengeId = 1 });

        _userChallengeRepositoryMock.Setup(uc => uc.CreateAsync(It.IsAny<UserChallenge>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.CommitAsync()).ReturnsAsync(1);

        // Act
        var result = await _challengeService.CreateChallengeAsync(1, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ChallengeId);

        _walletRepositoryMock.Verify(w => w.Update(It.IsAny<Wallet>()), Times.Once);
        _challengeRepositoryMock.Verify(c => c.CreateAsync(It.IsAny<Challenge>()), Times.Once);
        _userChallengeRepositoryMock.Verify(uc => uc.CreateAsync(It.IsAny<UserChallenge>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Exactly(2));
    }


    [Theory]
    [InlineData(ChallengeStatus.Canceled, typeof(InvalidOperationException),
        "This challenge has been canceled and cannot be joined.")]
    [InlineData(ChallengeStatus.Active, typeof(ArgumentException), "Invalid tree selection!")]
    public async Task JoinChallengeAsync_ShouldThrowException_IfConditionsNotMet(ChallengeStatus status,
        Type expectedException, string expectedMessage)
    {
        var challenge = new Challenge { ChallengeId = 1, Status = status };
        _challengeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(challenge);
        _userTreeRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((UserTree?)null);

        var exception = await Assert.ThrowsAsync(expectedException, async () =>
            await _challengeService.JoinChallengeAsync(1, 1, new JoinChallengeDto()));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task JoinChallengeAsync_ShouldThrowArgumentException_IfUserTreeIsInvalid()
    {
        const int userId = 1;
        const int challengeId = 10;
        var invalidJoinChallengeDto = new JoinChallengeDto { UserTreeId = 999 };

        _challengeRepositoryMock.Setup(repo => repo.GetByIdAsync(challengeId)).ReturnsAsync(new Challenge
            { ChallengeId = challengeId, Status = ChallengeStatus.Active });
        _userTreeRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((UserTree?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _challengeService.JoinChallengeAsync(userId, challengeId, invalidJoinChallengeDto));
    }

    [Fact]
    public async Task JoinChallengeAsync_ShouldThrowInvalidOperationException_IfChallengeIsCanceled()
    {
        const int userId = 1;
        const int challengeId = 10;
        var validJoinChallengeDto = new JoinChallengeDto { UserTreeId = 1 };

        _challengeRepositoryMock.Setup(repo => repo.GetByIdAsync(challengeId)).ReturnsAsync(new Challenge
            { ChallengeId = challengeId, Status = ChallengeStatus.Canceled });
        _userTreeRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new UserTree
        {
            UserTreeId = 1,
            UserId = userId,
            Name = "Zen Tree",
            TreeXpConfig = new TreeXpConfig { LevelId = 1, XpThreshold = 100 },
            User = new Users { UserId = userId, UserName = "TestUser" },
            TreeOwner = new Users { UserId = userId, UserName = "TestUser" }
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _challengeService.JoinChallengeAsync(userId, challengeId, validJoinChallengeDto));
    }

    [Fact]
    public async Task GetAllChallengesAsync_ShouldReturnList()
    {
        _mapperMock.Setup(m => m.Map<List<ChallengeDto>>(It.IsAny<List<Challenge>>()))
            .Returns((List<Challenge> source) => source.Select(c => new ChallengeDto
            {
                ChallengeId = c.ChallengeId,
                ChallengeName = c.ChallengeName
            }).ToList());

        // Act
        var result = await _challengeService.GetAllChallengesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetChallengeByIdAsync_ShouldReturnChallenge()
    {
        _mapperMock.Setup(m => m.Map<ChallengeDto>(It.IsAny<Challenge>()))
            .Returns((Challenge src) => new ChallengeDto
            {
                ChallengeId = src.ChallengeId,
                ChallengeName = src.ChallengeName
            });

        // Act
        var result = await _challengeService.GetChallengeByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ChallengeId);
    }

    [Fact]
    public async Task UpdateChallengeAsync_ShouldThrowException_WhenChallengeNotFound()
    {
        _challengeRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Challenge?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _challengeService.UpdateChallengeAsync(new UpdateChallengeDto { ChallengeId = 1 }));
    }

    [Fact]
    public async Task LeaveChallengeAsync_ShouldThrowException_WhenUserNotInChallenge()
    {
        _userChallengeRepositoryMock.Setup(r => r.GetUserChallengeAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((UserChallenge?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _challengeService.LeaveChallengeAsync(1, 1));
    }

    [Fact]
    public async Task CancelChallengeAsync_ShouldThrowException_WhenChallengeNotFound()
    {
        _challengeRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Challenge?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _challengeService.CancelChallengeAsync(1, 1));
    }

    [Fact]
    public async Task GetChallengeRankingsAsync_ShouldReturnList()
    {
        _userChallengeRepositoryMock.Setup(r => r.GetRankedUserChallengesAsync(It.IsAny<int>()))
            .ReturnsAsync([new UserChallenge { UserId = 1, Progress = 50 }]);

        var result = await _challengeService.GetChallengeRankingsAsync(1);

        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetUserChallengeProgressAsync_ShouldReturnProgress()
    {
        // Set up user challenge repository mock to return a UserChallenge
        var userChallenge = new UserChallenge
        {
            UserId = 1,
            ChallengeId = 1,
            Progress = 50
        };

        _userChallengeRepositoryMock.Setup(r => r.GetUserProgressAsync(1, 1))
            .ReturnsAsync(userChallenge);

        _mapperMock.Setup(m => m.Map<UserChallengeProgressDto>(It.IsAny<UserChallenge>()))
            .Returns(new UserChallengeProgressDto
            {
                UserId = 1,
                ChallengeId = 1,
                Progress = 50
            });

        // Act
        var result = await _challengeService.GetUserChallengeProgressAsync(1, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.Progress);
    }

    [Fact]
    public async Task VerifyMockReturnsData()
    {
        var challenges = await _challengeRepositoryMock.Object.GetChallengeAll();
        Assert.NotNull(challenges);
        Assert.NotEmpty(challenges);
    }

    [Fact]
    public async Task CreateChallengeAsync_ShouldThrowKeyNotFoundException_WhenUserNotFound()
    {
        // Arrange
        const int userId = 1;
        var request = new CreateChallengeDto { Reward = 100 };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
            .ReturnsAsync((Users?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _challengeService.CreateChallengeAsync(userId, request));
    }


    [Fact]
    public async Task JoinChallengeAsync_ShouldThrowKeyNotFoundException_WhenChallengeNotFound()
    {
        // Arrange
        _challengeRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Challenge?)null);

        var userId = 1;
        var challengeId = 10;
        var joinChallengeDto = new JoinChallengeDto();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _challengeService.JoinChallengeAsync(userId, challengeId, joinChallengeDto));
    }


    [Fact]
    public async Task LeaveChallengeAsync_ShouldThrowKeyNotFoundException_WhenUserNotInChallenge()
    {
        // Arrange
        const int userId = 1;
        const int challengeId = 10;

        var challenge = new Challenge
        {
            ChallengeId = challengeId,
            UserChallenges = new List<UserChallenge>()
        };

        _challengeRepositoryMock.Setup(repo => repo.GetByIdAsync(challengeId))
            .ReturnsAsync(challenge);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _challengeService.LeaveChallengeAsync(userId, challengeId));
    }

    [Fact]
    public async Task CancelChallengeAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotOrganizer()
    {
        // Arrange
        const int anotherUserId = 2;
        const int challengeId = 10;

        var challenge = new Challenge
        {
            ChallengeId = challengeId,
            Status = ChallengeStatus.Active
        };

        var nonOrganizerUserChallenge = new UserChallenge
        {
            UserId = anotherUserId,
            ChallengeId = challengeId,
            ChallengeRole = UserChallengeRole.Participant
        };

        _challengeRepositoryMock.Setup(repo => repo.GetByIdAsync(challengeId))
            .ReturnsAsync(challenge);

        _userChallengeRepositoryMock.Setup(repo => repo.GetUserChallengeAsync(anotherUserId, challengeId))
            .ReturnsAsync(nonOrganizerUserChallenge);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _challengeService.CancelChallengeAsync(challengeId, anotherUserId));
    }
}