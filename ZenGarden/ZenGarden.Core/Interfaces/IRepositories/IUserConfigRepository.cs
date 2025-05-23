﻿using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories;

public interface IUserConfigRepository : IGenericRepository<UserConfig>
{
    Task<UserConfig?> GetUserConfigbyUserIdAsync(int userId);
}