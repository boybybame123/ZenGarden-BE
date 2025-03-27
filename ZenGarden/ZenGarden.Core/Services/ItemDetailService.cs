using AutoMapper;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Services;

public class ItemDetailService(
    IItemDetailRepository itemDetailRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IS3Service s3Service)
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

    public async Task<List<ItemDetail>> GetAllItemDetails()
    {
        var itemDetails = await itemDetailRepository.GetAllAsync();
        return mapper.Map<List<ItemDetail>>(itemDetails);
    }


    public async Task UpdateItemDetailAsync(UpdateItemDetailDto itemDetail)
    {
        var updateItemDetail = await itemDetailRepository.GetItemDetailsByItemId(itemDetail.ItemId);

        if (updateItemDetail != null)
        {

            if (itemDetail.Description != null)
                updateItemDetail.Description = itemDetail.Description;
            if (itemDetail.Duration.HasValue)
                updateItemDetail.Duration = itemDetail.Duration;
            if (itemDetail.Effect != null)
                updateItemDetail.Effect = itemDetail.Effect;
            updateItemDetail.UpdatedAt = DateTime.Now;
            if (itemDetail.IsUnique != updateItemDetail.IsUnique)
                updateItemDetail.IsUnique = itemDetail.IsUnique;
            if (itemDetail.MonthlyPurchaseLimit.HasValue)
                updateItemDetail.MonthlyPurchaseLimit = itemDetail.MonthlyPurchaseLimit;

            if (itemDetail.File != null)
            {
                var mediaUrl = await s3Service.UploadFileAsync(itemDetail.File);
                if (mediaUrl != null) updateItemDetail.MediaUrl = mediaUrl;
            }

            itemDetailRepository.Update(updateItemDetail);
            if (await unitOfWork.CommitAsync() == 0)
                throw new InvalidOperationException("Failed to update item.");
        }
    }
}