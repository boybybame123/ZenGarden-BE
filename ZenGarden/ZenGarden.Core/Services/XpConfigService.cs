using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class XpConfigService(
    ITaskTypeRepository taskTypeRepository,
    IXpConfigRepository xpConfigRepository,
    IFocusMethodRepository focusMethodRepository,
    IUnitOfWork unitOfWork)
    : IXpConfigService
{
    public async Task EnsureXpConfigExists(int focusMethodId, int taskTypeId, int totalDuration)
    {
        if (taskTypeId <= 0 || focusMethodId <= 0 || totalDuration <= 0)
            throw new ArgumentException("Invalid TaskTypeId, FocusMethodId, or TotalDuration.");

        var taskType = await taskTypeRepository.GetByIdAsync(taskTypeId);
        var focusMethod = await focusMethodRepository.GetByIdAsync(focusMethodId);

        if (taskType == null || focusMethod == null)
            throw new KeyNotFoundException("TaskType or FocusMethod not found.");

        var newBaseXp = (double)totalDuration / 10 * taskType.XpMultiplier * focusMethod.XpMultiplier;
        const double epsilon = 0.0001;

        var existingXpConfig = await xpConfigRepository.GetByFocusMethodIdAndTaskTypeIdAsync(focusMethodId, taskTypeId);

        if (existingXpConfig == null)
        {
            var xpConfig = new XpConfig
            {
                FocusMethodId = focusMethodId,
                TaskTypeId = taskTypeId,
                BaseXp = newBaseXp,
                XpMultiplier = 1,
                PriorityDecayRate = 0.1,
                MinDecayMultiplier = 0.3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await xpConfigRepository.CreateAsync(xpConfig);
            await unitOfWork.CommitAsync();
        }
        else if (Math.Abs(existingXpConfig.BaseXp - newBaseXp) > epsilon)
        {
            existingXpConfig.BaseXp = newBaseXp;
            existingXpConfig.UpdatedAt = DateTime.UtcNow;
            xpConfigRepository.Update(existingXpConfig);
            await unitOfWork.CommitAsync();
        }
    }
}