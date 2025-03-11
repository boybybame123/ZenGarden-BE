using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UserTreeService(IUnitOfWork unitOfWork, IUserTreeRepository userTreeRepository, IMapper mapper)
    : IUserTreeService
{
    public async Task<IEnumerable<UserTree>> GetAllAsync()
    {
        return await userTreeRepository.GetAllAsync();
    }

    public async Task<UserTree> GetByIdAsync(int id)
    {
        return await userTreeRepository.GetByIdAsync(id)
               ?? throw new KeyNotFoundException("UserTree not found");
    }

    public async Task AddAsync(UserTreeDto userTreeDto)
    {
        var userTree = mapper.Map<UserTree>(userTreeDto);
        userTree.CreatedAt = DateTime.UtcNow;
        userTree.UpdatedAt = DateTime.UtcNow;

        await userTreeRepository.CreateAsync(userTree);
        await unitOfWork.CommitAsync();
    }

    public async Task UpdateAsync(int id, UserTreeDto userTreeDto)
    {
        var existingUserTree = await userTreeRepository.GetByIdAsync(id);
        if (existingUserTree == null) throw new KeyNotFoundException("UserTree not found");

        mapper.Map(userTreeDto, existingUserTree);
        existingUserTree.UpdatedAt = DateTime.UtcNow;

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
}