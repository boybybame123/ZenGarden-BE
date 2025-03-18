using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class ItemDetailService(IItemDetailRepository itemDetailRepository, IUnitOfWork unitOfWork, IMapper mapper, IS3Service s3Service)
    : IItemDetailService
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


    public async Task UpdateItemDetailAsync(UpdateItemDetailDto itemDetail)
    {
        var updateItemDetail = await itemDetailRepository.GetItemDetailsByItemId(itemDetail.ItemId);

        updateItemDetail.Type = itemDetail.Type;
        updateItemDetail.Description = itemDetail.Description;
        updateItemDetail.Duration = itemDetail.Duration;
        updateItemDetail.Effect = itemDetail.Effect;

        var mediaUrl = await s3Service.UploadFileAsync(itemDetail.File);
        updateItemDetail.MediaUrl = mediaUrl;

        itemDetailRepository.Update(updateItemDetail);
        if (await unitOfWork.CommitAsync() == 0)
            throw new InvalidOperationException("Failed to update item.");
    }
}