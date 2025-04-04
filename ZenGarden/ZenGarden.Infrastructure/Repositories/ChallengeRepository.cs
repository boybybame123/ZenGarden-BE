﻿using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class ChallengeRepository(ZenGardenContext context)
    : GenericRepository<Challenge>(context), IChallengeRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<List<Challenge>> GetChallengeAll()
    {
        return await _context.Challenge
            .AsNoTracking()
            .Include(c => c.ChallengeType)  
            .Include(c => c.UserChallenges)
            .Include(c => c.ChallengeTasks)
            .ThenInclude(ct => ct.Tasks)
            .ToListAsync();
    }

    public async Task<Challenge?> GetByIdChallengeAsync(int id)
    {
        return await _context.Challenge
            .AsNoTracking()
            .Include(c => c.ChallengeType)
            .Include(c => c.UserChallenges)
            .Include(c => c.ChallengeTasks)
            .ThenInclude(ct => ct.Tasks)
            .Where(c => c.ChallengeId == id)
            .FirstOrDefaultAsync();
    }
}