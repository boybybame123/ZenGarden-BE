using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class UserXpConfigService(
    IUserXpConfigRepository userXpConfigRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IUserXpConfigService
{
    public async Task CreateUserXpConfigAsync(UserXpConfigDto userXpConfig)
    {
        var newUserXp = mapper.Map<UserXpConfig>(userXpConfig);

        await userXpConfigRepository.CreateAsync(newUserXp);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create item.");
    }

    public async Task DeleteUserXpConfigAsync(int userXpConfigId)
    {
        var i = await userXpConfigRepository.GetByIdAsync(userXpConfigId);

        if (i == null)
            throw new KeyNotFoundException($"Item with ID {userXpConfigId} not found.");
        await userXpConfigRepository.RemoveAsync(i);
    }

    public async Task<UserXpConfig?> GetUserXpConfigByIdAsync(int userXpConfigId)
    {
        var i = await userXpConfigRepository.GetByIdAsync(userXpConfigId);
        return i;
    }

    public async Task UpdateUserXpConfigAsync(UserXpConfigDto userXpConfig)
    {
        var i = await userXpConfigRepository.GetByIdAsync(userXpConfig.LevelId);

        if (i == null)
            throw new KeyNotFoundException($"Item with ID {userXpConfig.LevelId} not found.");
        if (i.XpThreshold >= 0) i.XpThreshold = userXpConfig.XpThreshold;

        userXpConfigRepository.Update(i);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update item.");
    }

    public async Task<List<UserXpConfigDto>> GetAllUserXpConfigsAsync()
    {
        var i = await userXpConfigRepository.GetAllAsync();
        try
        {
            mapper.Map<List<UserXpConfigDto>>(i);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }


        return mapper.Map<List<UserXpConfigDto>>(i);
    }
}