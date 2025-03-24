using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IRepositories
{
    public interface IUserConfigRepository: IGenericRepository<UserConfig>
    {
        Task<UserConfig?> GetUserConfigbyUserIdAsync(int userId);
    }
}
