using AutoMapper;
using System.Threading.Tasks;
using ZenGarden.Core.Interfaces.IRepositories;
using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.Enums;

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


    public async Task<ItemDetail> UpdateItemDetailAsync(UpdateItemDetailDto itemDetail)
    {
        try
        {
            if (itemDetail == null)
            {
                throw new ArgumentNullException(nameof(itemDetail));
            }

            var updateItemDetail = await itemDetailRepository.GetItemDetailsByItemId(itemDetail.ItemId);
            if (updateItemDetail == null)
            {
                throw new KeyNotFoundException($"ItemDetail with ItemId {itemDetail.ItemId} not found.");
            }

            // Validate effect if provided
            if (itemDetail.Effect != null)
            {
                if (string.IsNullOrWhiteSpace(itemDetail.Effect))
                {
                    throw new ArgumentException("Effect cannot be empty or whitespace.");
                }

                // Try parse effect as number
                if (int.TryParse(itemDetail.Effect, out int effectValue))
                {
                    if (effectValue <= 0 || effectValue >= 100)
                    {
                        throw new ArgumentException("Effect value must be greater than 0 and less than 100.");
                    }
                }
                else
                {
                    throw new ArgumentException("Effect must be a valid number.");
                }

                updateItemDetail.Effect = itemDetail.Effect;
            }

            // Update other properties if provided
            if (itemDetail.Description != null)
                updateItemDetail.Description = itemDetail.Description;
            
            if (itemDetail.Duration.HasValue)
                updateItemDetail.Duration = itemDetail.Duration;
            
            updateItemDetail.UpdatedAt = DateTime.Now;
            
            if (itemDetail.IsUnique != updateItemDetail.IsUnique)
                updateItemDetail.IsUnique = itemDetail.IsUnique;
            
            if (itemDetail.MonthlyPurchaseLimit.HasValue)
                updateItemDetail.MonthlyPurchaseLimit = itemDetail.MonthlyPurchaseLimit;

            // Handle file upload if provided
            if (itemDetail.File != null)
            {
                try
                {
                    var mediaUrl = await s3Service.UploadFileAsync(itemDetail.File);
                    updateItemDetail.MediaUrl = mediaUrl;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to upload file to S3.", ex);
                }
            }

            itemDetailRepository.Update(updateItemDetail);
            
            if (await unitOfWork.CommitAsync() == 0)
            {
                throw new InvalidOperationException($"Failed to update item detail for ItemId {itemDetail.ItemId}");
            }

            return updateItemDetail;
        }
        catch (Exception ex) when (ex is not ArgumentNullException && ex is not KeyNotFoundException && ex is not ArgumentException)
        {
            throw new InvalidOperationException($"Error updating item detail: {ex.Message}", ex);
        }
    }

    public string GetFolderNameByItemType(ItemType type)
    {
        return type switch
        {
            ItemType.XpProtect => "Farming",
            ItemType.XpBoostTree => "Farming",
            ItemType.Avatar => "avatar",
            ItemType.Background => "background",
            ItemType.Music => "music",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }


    public async Task<int> EffectItem(int itemId)
    {
        var itemDetail = await itemDetailRepository.GetItemDetailsByItemId(itemId);
        int effect =itemDetail.Effect != null && int.TryParse(itemDetail.Effect, out int parsedEffect) ? parsedEffect : 0;
        return effect;
    }


}