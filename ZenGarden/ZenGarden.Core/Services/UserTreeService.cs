using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UserTreeService(
    IUnitOfWork unitOfWork,
    IUserTreeRepository userTreeRepository,
    ITreeRepository treeRepository,
    ITreeXpConfigRepository treeXpConfigRepository,
    ITreeXpLogRepository treeXpLogRepository,
    ITaskRepository taskRepository,
    IBagRepository bagRepository,
    IBagItemRepository bagItemRepository,
    IMapper mapper)
    : IUserTreeService
{
    public async Task<List<UserTreeDto>> GetAllUserTreesAsync()
    {
        var userTrees = await userTreeRepository.GetAllUserTreesAsync();
        return mapper.Map<List<UserTreeDto>>(userTrees);
    }


    public async Task<UserTreeDto> GetUserTreeDetailAsync(int userTreeId)
    {
        var userTree = await userTreeRepository.GetUserTreeDetailAsync(userTreeId)
                       ?? throw new KeyNotFoundException("UserTree not found");

        var maxLevelConfig = await treeXpConfigRepository.GetMaxLevelConfigAsync();
        var nextLevelConfig = await treeXpConfigRepository.GetNextLevelConfigAsync(userTree.LevelId);

        var userTreeDto = mapper.Map<UserTreeDto>(userTree);

        if (userTree.IsMaxLevel && maxLevelConfig != null)
        {
            userTreeDto.LevelId = maxLevelConfig.LevelId + 1;
            userTreeDto.XpToNextLevel = 0;
        }
        else if (nextLevelConfig != null)
        {
            userTreeDto.XpToNextLevel = nextLevelConfig.XpThreshold - userTree.TotalXp;
        }
        else
        {
            userTreeDto.XpToNextLevel = 0;
        }

        return userTreeDto;
    }


    public async Task AddAsync(CreateUserTreeDto createUserTreeDto)
    {
        var userTree = mapper.Map<UserTree>(createUserTreeDto);
        userTree.CreatedAt = DateTime.UtcNow;
        userTree.UpdatedAt = DateTime.UtcNow;
        userTree.LevelId = 1;
        userTree.TotalXp = 0;
        userTree.IsMaxLevel = false;
        userTree.TreeStatus = TreeStatus.Seed;
        userTree.TreeOwnerId = createUserTreeDto.UserId;

        await userTreeRepository.CreateAsync(userTree);
        await unitOfWork.CommitAsync();

        var defaultTasks = new List<Tasks>
        {
            new()
            {
                TaskName = "Morning Check-in",
                TaskDescription = "Write down your goals for the day",
                TaskTypeId = 1,
                UserTreeId = userTree.UserTreeId,
                FocusMethodId = 1,
                TotalDuration = 30,
                WorkDuration = 25,
                BreakTime = 5,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Status = TasksStatus.NotStarted,
                IsSuggested = true
            },
            new()
            {
                TaskName = "Focused Study",
                TaskDescription = "Spend 30 minutes studying your main subject",
                TaskTypeId = 1,
                UserTreeId = userTree.UserTreeId,
                FocusMethodId = 1,
                TotalDuration = 30,
                WorkDuration = 25,
                BreakTime = 5,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Status = TasksStatus.NotStarted,
                IsSuggested = true
            },
            new()
            {
                TaskName = "Relaxation Time",
                TaskDescription = "Listen to music or take a short walk for 20 minutes",
                TaskTypeId = 1,
                UserTreeId = userTree.UserTreeId,
                FocusMethodId = 1,
                TotalDuration = 20,
                WorkDuration = 15,
                BreakTime = 5,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Status = TasksStatus.NotStarted,
                IsSuggested = true
            },
            new()
            {
                TaskName = "End-of-Day Reflection",
                TaskDescription = "Review your day and evaluate your productivity",
                TaskTypeId = 1,
                UserTreeId = userTree.UserTreeId,
                FocusMethodId = 1,
                TotalDuration = 25,
                WorkDuration = 20,
                BreakTime = 5,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Status = TasksStatus.NotStarted,
                IsSuggested = true
            }
        };

        foreach (var task in defaultTasks) await taskRepository.CreateAsync(task);

        await unitOfWork.CommitAsync();
    }


    public async Task UpdateAsync(int id, CreateUserTreeDto createUserTreeDto)
    {
        var existingUserTree = await userTreeRepository.GetByIdAsync(id);
        if (existingUserTree == null) throw new KeyNotFoundException("UserTree not found");

        if (!string.IsNullOrWhiteSpace(createUserTreeDto.Name))
            existingUserTree.Name = createUserTreeDto.Name;

        existingUserTree.UpdatedAt = DateTime.UtcNow;

        if (existingUserTree is { IsMaxLevel: true, FinalTreeId: null })
            existingUserTree.FinalTreeId = await AssignRandomFinalTreeIdAsync();

        userTreeRepository.Update(existingUserTree);
        await unitOfWork.CommitAsync();
    }

    public async Task ChangeStatusAsync(int id, TreeStatus newStatus)
    {
        var existingUserTree = await userTreeRepository.GetByIdAsync(id);
        if (existingUserTree == null) throw new KeyNotFoundException("UserTree not found");

        existingUserTree.TreeStatus = newStatus;
        existingUserTree.UpdatedAt = DateTime.UtcNow;

        userTreeRepository.Update(existingUserTree);
        await unitOfWork.CommitAsync();
    }

    public async Task CheckAndSetMaxLevelAsync(UserTree userTree)
    {
        while (true)
        {
            var nextLevelConfig = await treeXpConfigRepository.GetNextLevelConfigAsync(userTree.LevelId);
            if (nextLevelConfig == null || userTree.TotalXp < nextLevelConfig.XpThreshold)
                break;

            userTree.LevelId = nextLevelConfig.LevelId;
        }

        if (userTree is { TreeStatus: TreeStatus.Seed, TotalXp: > 0 })
            userTree.TreeStatus = TreeStatus.Growing;

        var maxLevelConfig = await treeXpConfigRepository.GetMaxLevelConfigAsync();
        if (maxLevelConfig != null && userTree.TotalXp >= maxLevelConfig.XpThreshold)
        {
            userTree.TreeStatus = TreeStatus.MaxLevel;
            userTree.IsMaxLevel = true;

            var finalTreeId = await treeRepository.GetRandomFinalTreeIdAsync();
            if (finalTreeId != null)
                userTree.FinalTreeId = finalTreeId;
        }
    }


    public async Task<List<UserTreeDto>> GetAllUserTreesByUserIdAsync(int userId)
    {
        var userTrees = await userTreeRepository.GetUserTreeByUserIdAsync(userId);
        var maxLevelConfig = await treeXpConfigRepository.GetMaxLevelConfigAsync();

        var userTreeDtos = new List<UserTreeDto>();

        foreach (var userTree in userTrees)
        {
            var userTreeDto = mapper.Map<UserTreeDto>(userTree);
            var nextLevelConfig = await treeXpConfigRepository.GetNextLevelConfigAsync(userTree.LevelId);

            if (userTree.IsMaxLevel && maxLevelConfig != null)
            {
                userTreeDto.LevelId = maxLevelConfig.LevelId + 1;
                userTreeDto.XpToNextLevel = 0;
            }
            else if (nextLevelConfig != null)
            {
                userTreeDto.XpToNextLevel = nextLevelConfig.XpThreshold - userTree.TotalXp;
            }
            else
            {
                userTreeDto.XpToNextLevel = 0;
            }

            userTreeDtos.Add(userTreeDto);
        }

        return userTreeDtos;
    }

    public async Task UpdateSpecificTreeHealthAsync(int userTreeId)
    {
        const int dailyXpDecayRate = 10;

        var userTree = await userTreeRepository.GetByIdAsync(userTreeId);
        if (userTree == null
            || userTree.TreeStatus == TreeStatus.Withered
            || userTree.TreeStatus == TreeStatus.MaxLevel
            || userTree.TreeStatus == TreeStatus.Seed)
            return;

        var activeTask = await taskRepository.GetActiveTaskByUserTreeIdAsync(userTreeId);
        if (activeTask == null ||
            (activeTask.Status != TasksStatus.InProgress && activeTask.Status != TasksStatus.Paused))
            return;

        var lastUpdatedDate = userTree.UpdatedAt.Date;
        var currentDate = DateTime.UtcNow.Date;
        if (currentDate <= lastUpdatedDate)
            return;

        var daysSinceLastCheckIn = (currentDate - lastUpdatedDate).Days;
        var userId = userTree.UserId ?? throw new InvalidOperationException("UserId is null.");
        var itemBagId = await bagRepository.GetItemByHavingUse(userId, ItemType.Xp_protect);
        var itemBag = await bagItemRepository.GetByIdAsync(itemBagId);

        if (itemBag != null && itemBag.UpdatedAt.Date == lastUpdatedDate.AddDays(1)) daysSinceLastCheckIn -= 1;

        if (daysSinceLastCheckIn <= 0)
            return;
        var xpDecay = daysSinceLastCheckIn * dailyXpDecayRate;
        userTree.TotalXp = Math.Max(0, userTree.TotalXp - xpDecay);

        if (userTree.TotalXp == 0) userTree.TreeStatus = TreeStatus.Withered;

        userTree.UpdatedAt = DateTime.UtcNow;


        var log = new TreeXpLog
        {
            TaskId = activeTask.TaskId,
            XpAmount = -xpDecay,
            ActivityType = ActivityType.Decay,
            CreatedAt = DateTime.UtcNow
        };
        await treeXpLogRepository.CreateAsync(log);

        userTreeRepository.Update(userTree);
        await unitOfWork.CommitAsync();
    }


    public async Task<List<UserTree>> ListUserTreeByOwner(int ownerId)
    {
        var userTrees = await userTreeRepository.GetUserTreeByOwnerIdAsync(ownerId);
        return userTrees;
    }

    private async Task<int?> AssignRandomFinalTreeIdAsync()
    {
        var treeIds = await treeRepository.GetAllTreeIdsAsync();
        if (treeIds.Count == 0) return null;
        var random = new Random();
        return treeIds[random.Next(treeIds.Count)];
    }
}