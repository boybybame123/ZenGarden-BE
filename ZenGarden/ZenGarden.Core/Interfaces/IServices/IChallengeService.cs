using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Interfaces.IServices
{
    public interface IChallengeService
    {
        Task<List<Challenge>> GetAllChallengeAsync();
        Task<Challenge> GetChallengeByIdAsync(int ChallengeId);
        Task CreateChallengeAsync(ChallengeDto Challenge);
        Task UpdateChallengeAsync(ChallengeDto Challenge);
        Task DeleteChallengeAsync(int ChallengeId);
    }
}
