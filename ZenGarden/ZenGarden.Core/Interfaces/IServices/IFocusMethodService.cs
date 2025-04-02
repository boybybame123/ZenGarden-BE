using ZenGarden.Domain.DTOs;

namespace ZenGarden.Core.Interfaces.IServices;

public interface IFocusMethodService
{
    Task<FocusMethodDto> SuggestFocusMethodAsync(SuggestFocusMethodDto dto);
}