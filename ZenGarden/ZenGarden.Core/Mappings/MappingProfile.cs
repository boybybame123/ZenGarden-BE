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
        CreateMap<UserTree, CreateUserTreeDto>().ReverseMap();
        CreateMap<Tree, TreeDto>();
        CreateMap<Challenge, ChallengeDto>().ReverseMap();
        CreateMap<FocusMethod, FocusMethodDto>()
            .ForMember(dest => dest.FocusMethodName, opt => opt.MapFrom(src => src.Name));
        CreateMap<UserTree, UserTreeDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
            .ForMember(dest => dest.TreeStatus, opt => opt.MapFrom(src => src.TreeStatus.ToString()))
            .ForMember(dest => dest.FinalTreeName,
                opt => opt.MapFrom(src => src.FinalTree != null ? src.FinalTree.Name : null))
            .ForMember(dest => dest.FinalTreeRarity,
                opt => opt.MapFrom(src => src.FinalTree != null ? src.FinalTree.Rarity : null))
            .ForMember(dest => dest.XpToNextLevel, opt => opt.MapFrom(src => CalculateXpToNextLevel(src)));
        CreateMap<Tasks, TaskDto>()
            .ForMember(dest => dest.TaskTypeName, opt => opt.MapFrom(src => src.TaskType.TaskTypeName))
            .ForMember(dest => dest.FocusMethodName, opt => opt.MapFrom(src => src.FocusMethod.Name))
            .ForMember(dest => dest.UserTreeName, opt => opt.MapFrom(src => src.UserTree.Name));
        CreateMap<UserXpLog, UserXpLogDto>().ReverseMap();
        CreateMap<TaskType, TaskTypeDto>();
        CreateMap<CreateTaskTypeDto, TaskType>();
        CreateMap<UpdateTaskTypeDto, TaskType>()
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
    }

    private static double CalculateXpToNextLevel(UserTree userTree)
    {
        if (userTree.IsMaxLevel) return 0;

        return userTree.TreeXpConfig.XpThreshold - userTree.TotalXp;
    }
}