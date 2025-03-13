using ZenGarden.Core.Interfaces.IServices;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.DTOs.Response;

namespace ZenGarden.Core.Services;

public class FocusMethodService : IFocusMethodService
{
    public Task<SuggestFocusMethodResponse> SuggestFocusMethodAsync(SuggestFocusMethodDto requestDto)
    {
        throw new NotImplementedException();
    }
}