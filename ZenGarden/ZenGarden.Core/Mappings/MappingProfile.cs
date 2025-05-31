using AutoMapper;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.DTOs.FocusTracking;
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
        CreateMap<BagItem, BagItemDto>()
            .ReverseMap();
        CreateMap<CreateItemDetailDto, ItemDetail>();
        CreateMap<CreateItemDto, Item>().ReverseMap();
        CreateMap<ItemDetail, ItemDetailDto>()
            .ReverseMap();
        CreateMap<UserXpConfig, UserXpConfigDto>()
            .ReverseMap();
        CreateMap<Packages, PackageDto>().ReverseMap();
        CreateMap<UserTree, CreateUserTreeDto>().ReverseMap();
        CreateMap<Tree, TreeDto>().ReverseMap();
        CreateMap<Challenge, ChallengeDto>().ReverseMap();
        CreateMap<FocusMethod, FocusMethodDto>()
            .ForMember(dest => dest.FocusMethodName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        CreateMap<FocusMethod, FocusMethodWithReasonDto>()
            .IncludeBase<FocusMethod, FocusMethodDto>();
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
            .ForMember(dest => dest.UserTreeName, opt => opt.MapFrom(src => src.UserTree.Name))
            .ForMember(dest => dest.RemainingTime, opt => opt.Ignore());
        CreateMap<UserXpLog, UserXpLogDto>().ReverseMap();
        CreateMap<TaskType, TaskTypeDto>();
        CreateMap<CreateTaskTypeDto, TaskType>();
        CreateMap<UpdateTaskTypeDto, TaskType>()
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
        CreateMap<Challenge, ChallengeDto>()
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.ChallengeTasks.Select(ct => ct.Tasks)));
        CreateMap<CreateChallengeDto, Challenge>();
        CreateMap<TaskDto, Tasks>()
            .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
            .ForMember(dest => dest.TaskDescription, opt => opt.MapFrom(src => src.TaskDescription))
            .ForMember(dest => dest.TotalDuration, opt => opt.MapFrom(src => src.TotalDuration))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));

        CreateMap<CreateTaskDto, Tasks>()
            .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName))
            .ForMember(dest => dest.TaskDescription, opt => opt.MapFrom(src => src.TaskDescription))
            .ForMember(dest => dest.TotalDuration, opt => opt.MapFrom(src => src.TotalDuration))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.WorkDuration, opt => opt.MapFrom(src => src.WorkDuration))
            .ForMember(dest => dest.BreakTime, opt => opt.MapFrom(src => src.BreakTime))
            .ForMember(dest => dest.TaskTypeId, opt => opt.MapFrom(src => src.TaskTypeId))
            .ForMember(dest => dest.UserTreeId, opt => opt.MapFrom(src => src.UserTreeId))
            .ForMember(dest => dest.FocusMethodId, opt => opt.MapFrom(src => src.FocusMethodId));
        CreateMap<UpdateTaskDto, Tasks>();
        CreateMap<UserChallenge, UserChallengeProgressDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User != null ? src.User.UserName : "Unknown"))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Users, UserResponseDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : string.Empty));

        CreateMap<FocusActivity, FocusActivityDto>().ReverseMap();
        CreateMap<CreateFocusActivityDto, FocusActivity>();

        CreateMap<FocusTracking, FocusTrackingDto>()
            .ForMember(dest => dest.Activities, opt => opt.MapFrom(src => src.FocusActivities));
        CreateMap<CreateFocusTrackingDto, FocusTracking>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        CreateMap<UpdateFocusTrackingDto, FocusTracking>()
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
    }

    private static double CalculateXpToNextLevel(UserTree userTree)
    {
        if (userTree.IsMaxLevel) return 0;
        if (userTree.TreeXpConfig == null) return 0;

        return userTree.TreeXpConfig.XpThreshold - userTree.TotalXp;
    }
}