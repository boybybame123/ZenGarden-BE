using AutoMapper;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;

namespace ZenGarden.Core.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Users, UserDto>().ReverseMap();
        CreateMap<RegisterDto, Users>()
            .ForMember(dest => dest.Password, opt => opt.Ignore());
    }
}