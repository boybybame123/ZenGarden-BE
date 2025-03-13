using AutoMapper;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
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
        CreateMap<Item, ItemDto>()
            .ReverseMap();
        CreateMap<ItemDetail, ItemDetailDto>()
            .ReverseMap();
        CreateMap<UserXpConfig, UserXpConfigDto>()
            .ReverseMap();
        CreateMap<Packages, PackageDto>().ReverseMap();
        CreateMap<UserTree, UserTreeDto>().ReverseMap();
        CreateMap<Tree, TreeResponse>();
        CreateMap<TreeDto, Tree>();
        CreateMap<FocusMethod, FocusMethodDto>()
            .ForMember(dest => dest.FocusMethodName, opt => opt.MapFrom(src => src.Name));
    }
}