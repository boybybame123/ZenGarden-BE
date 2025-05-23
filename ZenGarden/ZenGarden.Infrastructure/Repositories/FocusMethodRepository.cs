using Microsoft.EntityFrameworkCore;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Infrastructure.Persistence;

namespace ZenGarden.Infrastructure.Repositories;

public class FocusMethodRepository(ZenGardenContext context, IRedisService redisService)
    : GenericRepository<FocusMethod>(context, redisService), IFocusMethodRepository
{
    private readonly ZenGardenContext _context = context;

    public async Task<List<string>> GetMethodNamesAsync()
    {
        return await _context.FocusMethod
            .Where(fm => fm.IsActive)
            .Select(fm => fm.Name)
            .ToListAsync();
    }

    public async Task<FocusMethodDto?> GetDtoByIdAsync(int id)
    {
        var focusMethod = await _context.FocusMethod
            .Where(f => f.FocusMethodId == id)
            .Select(f => new FocusMethodDto
            {
                FocusMethodId = f.FocusMethodId,
                FocusMethodName = f.Name,
                MinDuration = f.MinDuration,
                MaxDuration = f.MaxDuration,
                MinBreak = f.MinBreak,
                MaxBreak = f.MaxBreak,
                DefaultDuration = f.DefaultDuration,
                DefaultBreak = f.DefaultBreak,
                XpMultiplier = f.XpMultiplier
            })
            .FirstOrDefaultAsync();

        return focusMethod;
    }

    public async Task<FocusMethod?> SearchBySimilarityAsync(string methodName)
    {
        var methods = await _context.FocusMethod.ToListAsync();

        return methods
            .OrderBy(fm => LevenshteinDistance(fm.Name, methodName))
            .FirstOrDefault();
    }

    public async Task<FocusMethod?> GetByNameAsync(string name)
    {
        return await _context.FocusMethod
            .Where(fm => fm.Name == name)
            .FirstOrDefaultAsync();
    }

    private static int LevenshteinDistance(string s1, string s2)
    {
        if (s1 == s2) return 0;
        if (s1.Length == 0) return s2.Length;
        if (s2.Length == 0) return s1.Length;

        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (var i = 0; i <= s1.Length; i++) matrix[i, 0] = i;
        for (var j = 0; j <= s2.Length; j++) matrix[0, j] = j;

        for (var i = 1; i <= s1.Length; i++)
        for (var j = 1; j <= s2.Length; j++)
        {
            var cost = s2[j - 1] == s1[i - 1] ? 0 : 1;
            matrix[i, j] = Math.Min(
                Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                matrix[i - 1, j - 1] + cost
            );
        }

        return matrix[s1.Length, s2.Length];
    }
}