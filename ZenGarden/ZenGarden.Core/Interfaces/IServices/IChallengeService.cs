﻿using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IChallengeService
{
    Task<List<Challenge>> GetAllChallengeAsync();
    Task<Challenge> GetChallengeByIdAsync(int ChallengeId);
    Task<ChallengeDto> CreateChallengeAsync(int userId, CreateChallengeDto dto);
    Task UpdateChallengeAsync(ChallengeDto Challenge);
    Task DeleteChallengeAsync(int ChallengeId);
}