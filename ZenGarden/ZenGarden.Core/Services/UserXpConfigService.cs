using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class UserXpConfigService(
    IUserXpConfigRepository UserXpConfigRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IUserXpConfigService
{
    public async Task CreateUserXpConfigAsync(UserXpConfigDto UserXpConfig)
    {
        var newUserXp = mapper.Map<UserXpConfig>(UserXpConfig);

        await UserXpConfigRepository.CreateAsync(newUserXp);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to create item.");
    }

    public async Task DeleteUserXpConfigAsync(int UserXpConfigId)
    {
        var i = await UserXpConfigRepository.GetByIdAsync(UserXpConfigId);

        if (i == null)
            throw new KeyNotFoundException($"Item with ID {UserXpConfigId} not found.");
        await UserXpConfigRepository.RemoveAsync(i);
    }

    public async Task<UserXpConfig?> GetUserXpConfigByIdAsync(int UserXpConfigId)
    {
        var i = await UserXpConfigRepository.GetByIdAsync(UserXpConfigId);
        return i;
    }

    public async Task UpdateUserXpConfigAsync(UserXpConfigDto UserXpConfig)
    {
        var i = await UserXpConfigRepository.GetByIdAsync(UserXpConfig.LevelId);

        if (i == null)
            throw new KeyNotFoundException($"Item with ID {UserXpConfig.LevelId} not found.");
        if (i.XpThreshold != null && i.XpThreshold >= 0) i.XpThreshold = UserXpConfig.XpThreshold;

        UserXpConfigRepository.Update(i);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update item.");
    }

    public async Task<List<UserXpConfigDto>> GetAllUserXpConfigsAsync()
    {
        var i = await UserXpConfigRepository.GetAllAsync();
        try
        {
            var a = mapper.Map<List<UserXpConfigDto>>(i);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }


        return mapper.Map<List<UserXpConfigDto>>(i);
    }
}