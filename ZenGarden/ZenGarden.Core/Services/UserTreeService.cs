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

        var userTreeDto = mapper.Map<UserTreeDto>(userTree);
        await SetXpToNextLevelAsync(userTree, userTreeDto);

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
                EndDate = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1),
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
                EndDate = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1),
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
                EndDate = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1),
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
                EndDate = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1),
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
        var userTreeDtos = new List<UserTreeDto>();

        foreach (var userTree in userTrees)
        {
            var dto = mapper.Map<UserTreeDto>(userTree);
            await SetXpToNextLevelAsync(userTree, dto);
            userTreeDtos.Add(dto);
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

        var activeTask = await taskRepository.GetUserTaskInProgressAsync(userTreeId);
        if (activeTask is not { Status: TasksStatus.InProgress })
            return;

        var lastUpdatedDate = userTree.UpdatedAt.Date;
        var currentDate = DateTime.UtcNow.Date;
        if (currentDate <= lastUpdatedDate)
            return;

        var daysSinceLastCheckIn = (currentDate - lastUpdatedDate).Days;
        var userId = userTree.UserId ?? throw new InvalidOperationException("UserId is null.");
        var itemBagId = await bagRepository.GetItemByHavingUse(userId, ItemType.XpProtect);
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


    public async Task<List<UserTree>> GetAllUserTreesHavingMaxLevelByOwnerIdAsync(int userId)
    {
        var userTrees = await userTreeRepository.GetAllUserTreesHavingMaxLevelByOwnerIdAsync(userId);
        return userTrees;
    }

    public async Task<List<UserTreeDto>> GetActiveUserTreeAsync(int userId)
    {
        var userTrees = await userTreeRepository.GetActiveUserTreeAsync(userId);
        var userTreeDto = new List<UserTreeDto>();

        foreach (var userTree in userTrees)
        {
            var dto = mapper.Map<UserTreeDto>(userTree);

            // Lọc task InProgress hoặc Paused
            dto.Tasks = userTree.Tasks
                .Where(t => t.Status is TasksStatus.InProgress or TasksStatus.Paused)
                .Select(mapper.Map<TaskDto>)
                .ToList();

            await SetXpToNextLevelAsync(userTree, dto);
            userTreeDto.Add(dto);
        }

        return userTreeDto;
    }


    private async Task<int?> AssignRandomFinalTreeIdAsync()
    {
        var treeIds = await treeRepository.GetAllTreeIdsAsync();
        if (treeIds.Count == 0) return null;
        var random = new Random();
        return treeIds[random.Next(treeIds.Count)];
    }

    private async Task SetXpToNextLevelAsync(UserTree userTree, UserTreeDto dto)
    {
        var maxLevelConfig = await treeXpConfigRepository.GetMaxLevelConfigAsync();
        var nextLevelConfig = await treeXpConfigRepository.GetNextLevelConfigAsync(userTree.LevelId);

        if (userTree.IsMaxLevel && maxLevelConfig != null)
        {
            dto.LevelId = maxLevelConfig.LevelId + 1;
            dto.XpToNextLevel = 0;
        }
        else if (nextLevelConfig != null)
        {
            dto.XpToNextLevel = nextLevelConfig.XpThreshold - userTree.TotalXp;
        }
        else
        {
            dto.XpToNextLevel = 0;
        }
    }
}