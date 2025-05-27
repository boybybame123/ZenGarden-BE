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
    IMapper mapper,
    IUserRepository userRepository,
    IFocusMethodService focusMethodService,
    IUseItemService useItemService)
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
        var user = await userRepository.GetByIdAsync(createUserTreeDto.UserId)
                   ?? throw new KeyNotFoundException($"User with ID {createUserTreeDto.UserId} not found.");

        var defaultTreeXpConfig = await treeXpConfigRepository.GetByIdAsync(1)
                                 ?? throw new KeyNotFoundException("Default TreeXpConfig not found.");

        var newUserTree = new UserTree
        {
            Name = createUserTreeDto.Name,
            UserId = createUserTreeDto.UserId,
            TreeOwnerId = createUserTreeDto.UserId,
            LevelId = 1,
            TotalXp = 0,
            IsMaxLevel = false,
            TreeStatus = TreeStatus.Seed,
            TreeXpConfig = defaultTreeXpConfig,
            User = user,
            TreeOwner = user,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await userTreeRepository.CreateAsync(newUserTree);
        await unitOfWork.CommitAsync();

        // Create default tasks with AI-suggested focus methods
        var defaultTasks = new List<Tasks>
        {
            new()
            {
                TaskName = "Water your tree",
                TaskDescription = "Take care of your tree by watering it regularly",
                TaskTypeId = 1, // Daily task
                UserTreeId = newUserTree.UserTreeId,
                TotalDuration = 5,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1),
                CreatedAt = DateTime.UtcNow,
                Status = TasksStatus.NotStarted
            },
            new()
            {
                TaskName = "Prune your tree",
                TaskDescription = "Keep your tree healthy by pruning it",
                TaskTypeId = 1, 
                UserTreeId = newUserTree.UserTreeId,
                TotalDuration = 15,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                Status = TasksStatus.NotStarted
            },
            new()
            {
                TaskName = "Fertilize your tree",
                TaskDescription = "Give your tree nutrients to help it grow",
                TaskTypeId = 1,
                UserTreeId = newUserTree.UserTreeId,
                TotalDuration = 30,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                CreatedAt = DateTime.UtcNow,
                Status = TasksStatus.NotStarted
            },
            new()
            {
                TaskName = "Check tree health",
                TaskDescription = "Inspect your tree for any signs of disease or pests",
                TaskTypeId = 1,
                UserTreeId = newUserTree.UserTreeId,
                TotalDuration = 10,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1),
                CreatedAt = DateTime.UtcNow,
                Status = TasksStatus.NotStarted
            }
        };

        // Get AI suggestions for each task
        foreach (var task in defaultTasks)
        {
            var suggestedMethod = await focusMethodService.SuggestFocusMethodAsync(new SuggestFocusMethodDto
            {
                TaskName = task.TaskName,
                TaskDescription = task.TaskDescription,
                TotalDuration = task.TotalDuration,
                StartDate = task.StartDate ?? DateTime.UtcNow,
                EndDate = task.EndDate ?? DateTime.UtcNow.AddDays(1)
            });

            task.FocusMethodId = suggestedMethod.FocusMethodId;
            task.WorkDuration = suggestedMethod.DefaultDuration ?? 25;
            task.BreakTime = suggestedMethod.DefaultBreak ?? 5;
            task.IsSuggested = true;
        }

        // Create tasks one by one
        foreach (var task in defaultTasks)
        {
            await taskRepository.CreateAsync(task);
        }
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
        var userTreeDto = new List<UserTreeDto>();

        foreach (var userTree in userTrees)
        {
            var dto = mapper.Map<UserTreeDto>(userTree);
            await SetXpToNextLevelAsync(userTree, dto);
            userTreeDto.Add(dto);
        }

        return userTreeDto;
    }


    public async Task UpdateSpecificTreeHealthAsync(int userTreeId)
    {
        const int dailyXpDecayRate = 10;
        var shouldProcess = true;

        var userTree = await userTreeRepository.GetByIdAsync(userTreeId);
        if (userTree == null)
        {
            Console.WriteLine($"[TreeHealth] Tree ID {userTreeId} not found.");
            shouldProcess = false;
        }
        else if (userTree.TreeStatus is TreeStatus.Withered or TreeStatus.MaxLevel or TreeStatus.Seed)
        {
            Console.WriteLine($"[TreeHealth] Tree ID {userTreeId} has status {userTree.TreeStatus}, skipping decay.");
            shouldProcess = false;
        }

        var activeTask = await taskRepository.GetActiveTaskByUserTreeIdAsync(userTreeId);
        if (activeTask == null)
        {
            Console.WriteLine($"[TreeHealth] No active task for Tree ID {userTreeId}.");
            shouldProcess = false;
        }

        var lastUpdatedDate = userTree?.UpdatedAt.Date ?? DateTime.MinValue;
        var currentDate = DateTime.UtcNow.Date;
        if (currentDate <= lastUpdatedDate)
        {
            Console.WriteLine($"[TreeHealth] Tree ID {userTreeId} was already updated today.");
            shouldProcess = false;
        }

        var daysSinceLastCheckIn = (currentDate - lastUpdatedDate).Days;
        var userId = userTree?.UserId ?? -1;

        var itemBagId = await bagRepository.GetItemByHavingUse(userId, ItemType.XpProtect);
        var itemBag = await bagItemRepository.GetByIdAsync(itemBagId);

        if (itemBag != null && itemBag.UpdatedAt.Date == lastUpdatedDate.AddDays(1)&& itemBag.Quantity<=1)
        {
            await useItemService.UseItemXpProtect(userId);
            daysSinceLastCheckIn -= 1;
            Console.WriteLine($"[TreeHealth] XP Protect item used, effective days reduced to {daysSinceLastCheckIn}.");
        }

        if (daysSinceLastCheckIn <= 0)
        {
            Console.WriteLine($"[TreeHealth] No effective days since last update for Tree ID {userTreeId}.");
            shouldProcess = false;
        }

        if (shouldProcess)
        {
            var xpDecay = daysSinceLastCheckIn * dailyXpDecayRate;
            if (userTree != null)
            {
                var oldXp = userTree.TotalXp;

                userTree.TotalXp = Math.Max(0, userTree.TotalXp - xpDecay);
                userTree.UpdatedAt = DateTime.UtcNow;

                if (userTree.TotalXp == 0)
                {
                    userTree.TreeStatus = TreeStatus.Withered;
                    Console.WriteLine($"[TreeHealth] Tree ID {userTreeId} has withered due to XP = 0.");
                }

                var log = new TreeXpLog
                {
                    TaskId = activeTask?.TaskId,
                    XpAmount = -xpDecay,
                    ActivityType = ActivityType.Decay,
                    CreatedAt = DateTime.UtcNow
                };
                await treeXpLogRepository.CreateAsync(log);

                userTreeRepository.Update(userTree);
                await unitOfWork.CommitAsync();

                Console.WriteLine(
                    $"[TreeHealth] Tree ID {userTreeId} XP decayed from {oldXp} to {userTree.TotalXp} (Decay: {xpDecay}).");
            }
        }
        else
        {
            Console.WriteLine($"[TreeHealth] Skipping XP decay for Tree ID {userTreeId} due to previous checks.");
        }
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

    public async Task<UserTreeDto> GetUserTreeByIdAsync(int userTreeId)
    {
        var userTree = await userTreeRepository.GetByIdAsync(userTreeId)
                       ?? throw new KeyNotFoundException("UserTree not found");

        var userTreeDto = mapper.Map<UserTreeDto>(userTree);


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
    public async Task SetTreeWitheredAsync(int userTreeId)
    {
        var userTree = await userTreeRepository.GetByIdAsync(userTreeId)
                       ?? throw new KeyNotFoundException("UserTree not found");

        if (userTree.TreeStatus == TreeStatus.Withered)
            return;
       
        var ownerId = userTree.TreeOwnerId ?? -1;
        if (ownerId == -1)
        {
            userTree.TreeStatus = TreeStatus.Withered;
        }
        else
        {
            // Check if the user has an XpProtect item and use it if available
            var itemBagId = await bagRepository.GetItemByHavingUse(ownerId, ItemType.XpProtect);
            if (itemBagId != -1)
            {
                var itemBag = await bagItemRepository.GetByIdAsync(itemBagId);
                if (itemBag != null&& itemBag.Quantity <=1)
                {
                    await useItemService.UseItemXpProtect(ownerId);
                    // Mark the item as used (for example, update its UpdatedAt or status;
                    await unitOfWork.CommitAsync();
                    return;
                }
            }
            userTree.TreeStatus = TreeStatus.Withered;
        }
        
        userTree.UpdatedAt = DateTime.UtcNow;
        userTreeRepository.Update(userTree);
        await unitOfWork.CommitAsync();
    }
}