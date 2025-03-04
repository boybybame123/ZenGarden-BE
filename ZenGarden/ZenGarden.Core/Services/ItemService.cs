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
    public class ItemService(IItemRepository itemRepository, IUnitOfWork unitOfWork, IMapper mapper) : IItemService
    {
        public async Task<List<ItemDto>> GetAllItemsAsync()
        {
            var items = await itemRepository.GetAllItemAsync(); // Gọi đúng phương thức có Include
            return mapper.Map<List<ItemDto>>(items);
        }


        public async Task<Item?> GetItemByIdAsync(int itemId)
        {
            return await itemRepository.GetByIdAsync(itemId)
                   ?? throw new KeyNotFoundException($"Item with ID {itemId} not found.");
        }
        public async Task CreateItemAsync(Item item)
        {
            itemRepository.Create(item);
            if (await unitOfWork.CommitAsync() == 0)
                throw new InvalidOperationException("Failed to create item.");
        }
        public async Task UpdateItemAsync(ItemDto item)
        {
            var updateItem = await GetItemByIdAsync(item.ItemId);
            updateItem = mapper.Map(item, updateItem);
            itemRepository.Update(updateItem);
            if (await unitOfWork.CommitAsync() == 0)
                throw new InvalidOperationException("Failed to update item.");
        }
        public async Task DeleteItemAsync(int itemId)
        {
            var item = await GetItemByIdAsync(itemId);
            itemRepository.Remove(item);
            if (await unitOfWork.CommitAsync() == 0)
                throw new InvalidOperationException("Failed to delete item.");
        }
    }
}
