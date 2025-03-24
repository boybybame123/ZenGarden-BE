using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories
{
    public class UserConfigRepository(ZenGardenContext context)
    : GenericRepository<UserConfig>(context), IUserConfigRepository
    {
        private readonly ZenGardenContext _context = context;

        public async Task<UserConfig?> GetUserConfigbyUserIdAsync(int userId)
        {
            return await _context.UserConfig.FindAsync(userId);
        }



    }
}
