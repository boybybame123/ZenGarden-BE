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
        CreateMap<Item, ItemDto>().ReverseMap();
        CreateMap<ItemDetail, ItemDetailDto>().ReverseMap();
        CreateMap<Packages, PackageDto>().ReverseMap();

    }
}