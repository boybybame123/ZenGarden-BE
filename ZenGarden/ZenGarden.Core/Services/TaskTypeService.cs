using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class TaskTypeService(IUnitOfWork unitOfWork, IMapper mapper, ITaskTypeRepository taskTypeRepository)
    : ITaskTypeService
{
    public async Task<IEnumerable<TaskTypeDto>> GetAllTaskTypesAsync()
    {
        var taskTypes = await taskTypeRepository.GetAllAsync();
        return mapper.Map<IEnumerable<TaskTypeDto>>(taskTypes);
    }

    public async Task<TaskTypeDto?> GetTaskTypeByIdAsync(int id)
    {
        var taskType = await taskTypeRepository.GetByIdAsync(id);
        return taskType is not null ? mapper.Map<TaskTypeDto>(taskType) : null;
    }

    public async Task<TaskTypeDto> CreateTaskTypeAsync(CreateTaskTypeDto dto)
    {
        var taskType = mapper.Map<TaskType>(dto);
        await taskTypeRepository.CreateAsync(taskType);
        await unitOfWork.CommitAsync();
        return mapper.Map<TaskTypeDto>(taskType);
    }

    public async Task<bool> UpdateTaskTypeAsync(int id, UpdateTaskTypeDto dto)
    {
        var taskType = await taskTypeRepository.GetByIdAsync(id);
        if (taskType is null) return false;

        mapper.Map(dto, taskType);
        taskType.UpdatedAt = DateTime.UtcNow;

        taskTypeRepository.Update(taskType);
        return await unitOfWork.CommitAsync() > 0;
    }

    public async Task<bool> DeleteTaskTypeAsync(int id)
    {
        var taskType = await taskTypeRepository.GetByIdAsync(id);
        if (taskType is null) return false;

        await taskTypeRepository.RemoveAsync(taskType);
        return await unitOfWork.CommitAsync() > 0;
    }
}
