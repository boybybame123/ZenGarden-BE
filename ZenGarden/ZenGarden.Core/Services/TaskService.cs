using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class TaskService(ITaskRepository taskRepository, IUnitOfWork unitOfWork, IMapper mapper) : ITaskService
{
    public async Task<List<TaskDto>> GetAllUsersAsync()
    {
        var tasks = await taskRepository.GetAllAsync();
        return mapper.Map<List<TaskDto>>(tasks);
    }
    public async Task<Tasks?> GetTaskByIdAsync(int taskId)
    {
        return await taskRepository.GetByIdAsync(taskId)
               ?? throw new KeyNotFoundException($"Task with ID {taskId} not found.");
    }
    public async Task CreateTaskAsync(Tasks task)
    {
        taskRepository.Create(task);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create task.");
    }
    public async Task UpdateTaskAsync(TaskDto task)
    {
        var updateTask = await GetTaskByIdAsync(task.TaskId);
        updateTask = mapper.Map(task, updateTask);
        taskRepository.Update(updateTask);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update task.");
    }
    public async Task DeleteTaskAsync(int taskId)
    {
        var task = await GetTaskByIdAsync(taskId);
        taskRepository.Remove(task);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to delete task.");
    }
}