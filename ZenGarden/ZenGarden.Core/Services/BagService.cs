using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class BagService(IBagRepository bagRepository, IUnitOfWork unitOfWork, IMapper mapper) : IBagService
{
    public async Task<Bag?> GetBagByIdAsync(int BagId)
    {
        return await bagRepository.GetByIdAsync(BagId)
       ?? throw new KeyNotFoundException($"Item with ID {BagId} not found.");
    }


}
