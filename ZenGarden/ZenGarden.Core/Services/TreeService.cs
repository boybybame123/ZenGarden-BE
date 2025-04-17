using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class TreeService(ITreeRepository treeRepository, IMapper mapper, IUnitOfWork unitOfWork)
    : ITreeService
{
    public async Task<IEnumerable<TreeDto>> GetAllAsync()
    {
        var trees = await treeRepository.GetAllAsync();
        return mapper.Map<IEnumerable<TreeDto>>(trees);
    }

    public async Task<TreeDto?> GetByIdAsync(int id)
    {
        var tree = await treeRepository.GetByIdAsync(id)
                   ?? throw new KeyNotFoundException("Tree not found");

        return mapper.Map<TreeDto>(tree);
    }

    public async Task<TreeDto> AddAsync(TreeDto treeDto)
    {
        var tree = mapper.Map<Tree>(treeDto);
        tree.CreatedAt = DateTime.UtcNow;
        tree.IsActive = true;

        await treeRepository.CreateAsync(tree);
        await unitOfWork.CommitAsync();

        return mapper.Map<TreeDto>(tree);
    }

    public async Task UpdateAsync(int id, TreeDto treeDto)
    {
        var tree = await treeRepository.GetByIdAsync(id)
                   ?? throw new KeyNotFoundException("Tree not found");

        if (!tree.IsActive)
            throw new InvalidOperationException("Cannot update a disabled tree.");

        mapper.Map(treeDto, tree);
        tree.UpdatedAt = DateTime.UtcNow;

        treeRepository.Update(tree);
        await unitOfWork.CommitAsync();
    }


    public async Task<bool> DisableTreeAsync(int id)
    {
        var tree = await treeRepository.GetByIdAsync(id);
        if (tree == null) throw new KeyNotFoundException("Tree not found");

        if (!tree.IsActive)
            return false;

        tree.IsActive = false;
        tree.UpdatedAt = DateTime.UtcNow;

        treeRepository.Update(tree);
        await unitOfWork.CommitAsync();

        return true;
    }
}