using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IUserService
{
    Users? ValidateUser(string? email, string? phone, string password);
}