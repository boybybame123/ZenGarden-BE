using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IChallengeRepository : IGenericRepository<Challenge>
{
    Task<List<Challenge>> GetCallengeAll();


    Task<Challenge> GetByIdChallengeAsync(int id);
}