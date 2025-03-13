using AutoMapper;
using Newtonsoft.Json;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using ZenGarden.Domain.DTOs.Response;
using ZenGarden.Shared.Helpers;

namespace ZenGarden.Core.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Users, UserDto>().ReverseMap();
        CreateMap<RegisterDto, Users>()
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .AfterMap((src, dest) => dest.Password = PasswordHasher.HashPassword(src.Password));
        CreateMap<Item, ItemDto>().ReverseMap();
        CreateMap<ItemDetail, ItemDetailDto>()
            .ForMember(dest => dest.EffectData, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.Effect)
                ? JsonConvert.DeserializeObject<EffectData>(src.Effect)
                : new EffectData()))
            .ReverseMap()
            .ForMember(dest => dest.Effect, opt => opt.MapFrom(src =>
                JsonConvert.SerializeObject(src.EffectData)));
        CreateMap<Packages, PackageDto>().ReverseMap();
        CreateMap<UserTree, UserTreeDto>().ReverseMap();
        CreateMap<Tree, TreeResponse>();
        CreateMap<TreeDto, Tree>();
        CreateMap<FocusMethod, SuggestFocusMethodResponse>();
    }
}