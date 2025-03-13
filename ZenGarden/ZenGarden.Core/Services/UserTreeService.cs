using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

namespace ZenGarden.Core.Services;

public class UserTreeService(IUnitOfWork unitOfWork, IUserTreeRepository userTreeRepository, ITreeRepository treeRepository, IMapper mapper)
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
        userTree.LevelId = 1;
        userTree.TotalXp = 0;
        userTree.IsMaxLevel = false;

        await userTreeRepository.CreateAsync(userTree);
        await unitOfWork.CommitAsync();
    }


    public async Task UpdateAsync(int id, UserTreeDto userTreeDto)
    {
        var existingUserTree = await userTreeRepository.GetByIdAsync(id);
        if (existingUserTree == null) throw new KeyNotFoundException("UserTree not found");

        if (!string.IsNullOrWhiteSpace(userTreeDto.Name))
            existingUserTree.Name = userTreeDto.Name;

        if (userTreeDto.TreeStatus != default) 
            existingUserTree.TreeStatus = userTreeDto.TreeStatus;

        existingUserTree.UpdatedAt = DateTime.UtcNow;

        if (existingUserTree is { IsMaxLevel: true, FinalTreeId: null })
        {
            existingUserTree.FinalTreeId = await AssignRandomFinalTreeIdAsync();
        }

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

    private async Task<int?> AssignRandomFinalTreeIdAsync()
    {
        var treeIds = await treeRepository.GetAllTreeIdsAsync();
        if (treeIds.Count == 0) return null;
        var random = new Random();
        return treeIds[random.Next(treeIds.Count)];
    }
}