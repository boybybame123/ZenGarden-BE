using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class TreeService(ITreeRepository treeRepository, IMapper mapper, IUnitOfWork unitOfWork)
    : ITreeService
{
    public async Task<IEnumerable<TreeResponse>> GetAllAsync()
    {
        var trees = await treeRepository.GetAllAsync();
        return mapper.Map<IEnumerable<TreeResponse>>(trees);
    }

    public async Task<TreeResponse?> GetByIdAsync(int id)
    {
        var tree = await treeRepository.GetByIdAsync(id)
                   ?? throw new KeyNotFoundException("Tree not found");

        return mapper.Map<TreeResponse>(tree);
    }

    public async Task AddAsync(TreeDto treeDto)
    {
        var tree = mapper.Map<Tree>(treeDto);
        await treeRepository.CreateAsync(tree);
        await unitOfWork.CommitAsync();
    }

    public async Task UpdateAsync(int id, TreeDto treeDto)
    {
        var tree = await treeRepository.GetByIdAsync(id)
                   ?? throw new KeyNotFoundException("Tree not found");

        mapper.Map(treeDto, tree);
        tree.UpdatedAt = DateTime.UtcNow;
        treeRepository.Update(tree);
        await unitOfWork.CommitAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var tree = await treeRepository.GetByIdAsync(id)
                   ?? throw new KeyNotFoundException("Tree not found");

        await treeRepository.RemoveAsync(tree);
        await unitOfWork.CommitAsync();
    }
}