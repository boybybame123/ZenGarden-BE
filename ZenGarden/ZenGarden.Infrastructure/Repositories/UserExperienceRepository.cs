using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class UserExperienceRepository(ZenGardenContext context)
    : GenericRepository<UserExperience>(context), IUserExperienceRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<UserExperience?> GetByUserIdAsync(int userId)
    {
        return await _context.UserExperience.FirstOrDefaultAsync(x => x.UserId == userId);
    }
}