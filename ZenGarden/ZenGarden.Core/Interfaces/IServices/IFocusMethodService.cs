using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.DTOs.Response;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IFocusMethodService
{
    Task<SuggestFocusMethodResponse> SuggestFocusMethodAsync(SuggestFocusMethodDto requestDto);
}