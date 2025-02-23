using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUserRepository : IGenericRepository<Users>
{ 
    Task<Users?> ValidateUserAsync(string? email, string? phone, string password);
}