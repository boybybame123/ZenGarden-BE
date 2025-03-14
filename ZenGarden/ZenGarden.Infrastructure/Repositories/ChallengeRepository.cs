using Microsoft.EntityFrameworkCore;
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
    public class ChallengeRepository(ZenGardenContext context) : GenericRepository<Challenge>(context), IChallengeRepository
    {
        public async Task<List<Challenge>> GetCallengeAll()
        {
            return await context.Challenge
                .Include(u => u.ChallengeType)
                .Include(u => u.UserChallenges)
                .Include(u => u.ChallengeTasks)
                .AsNoTracking()
                .ToListAsync();

        }



        public async Task<Challenge?> GetByIdChallengeAsync(int id)
        {
            return await context.Challenge
                .Include(u => u.ChallengeType)
                .Include(u => u.UserChallenges)
                .Include(u => u.ChallengeTasks)
                .Where(fm => fm.ChallengeId == id)
                .FirstOrDefaultAsync();
        }
    }
}
