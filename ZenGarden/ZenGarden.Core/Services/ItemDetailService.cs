using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services
{
    public class ItemDetailService(IItemDetailRepository itemDetailRepository, IUnitOfWork unitOfWork, IMapper mapper) : IItemDetailService
    {
        

        public Task CreateItemDetailAsync(ItemDetail itemDetail)
        {
            throw new NotImplementedException();
        }

        public Task DeleteItemDetailAsync(int itemDetailId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ItemDetailDto>> GetAllItemDetails()
        {
           var itemDetails = await itemDetailRepository.GetAllAsync();
            return mapper.Map<List<ItemDetailDto>>(itemDetails);
        }



        public Task UpdateItemDetailAsync(ItemDetail itemDetail)
        {
            throw new NotImplementedException();
        }
    }

}
