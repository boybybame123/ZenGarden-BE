using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUserRepository
{
    Users? ValidateUser(string? email, string? phone, string password);
}