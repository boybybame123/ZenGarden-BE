using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;

namespace ZenGarden.Core.Services
{
    public class ChallengeService(IChallengeRepository challengeRepository,IUnitOfWork unitOfWork, IMapper mapper ) : IChallengeService
    {
    }
}
