using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class BagService(IBagRepository bagRepository, IUnitOfWork unitOfWork, IMapper mapper) : IBagService
{
    public async Task<Bag?> GetBagByIdAsync(int bagId)
    {
        return await bagRepository.GetByIdAsync(bagId)
               ?? throw new KeyNotFoundException($"Item with ID {bagId} not found.");
    }
}