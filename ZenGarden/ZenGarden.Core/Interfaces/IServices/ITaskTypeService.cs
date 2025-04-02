using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices;

public interface ITaskTypeService
{
    Task<IEnumerable<TaskTypeDto>> GetAllTaskTypesAsync();
    Task<TaskTypeDto?> GetTaskTypeByIdAsync(int id);
    Task<TaskTypeDto> CreateTaskTypeAsync(CreateTaskTypeDto dto);
    Task<bool> UpdateTaskTypeAsync(int id, UpdateTaskTypeDto dto);
    Task<bool> DeleteTaskTypeAsync(int id);
}