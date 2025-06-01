using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class BagService(IBagRepository bagRepository) : IBagService
{
    public async Task<Bag?> GetBagByIdAsync(int bagId)
    {
        return await bagRepository.GetByIdAsync(bagId)
               ?? throw new KeyNotFoundException($"bag with ID {bagId} not found.");
    }
    public async Task<bool?> GetBagItemAsync(int userid)
    {
       return await bagRepository.HasUsedXpBoostItemAsync(userid);
        
    }
}