using AutoMapper;
using AllocationSystem.WebApi.Models;

namespace AllocationSystem.WebApi;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<BaseEntityDto, BaseEntity>().ReverseMap();

        CreateMap<Topic, TopicDto>();

        CreateMap<UserDto, User>();

        CreateMap<User, UserResponseDto>().ReverseMap();

        CreateMap<AdminSetting, AdminSettingResponseDto>().ForMember(dest => dest.IsTopicMultiple, opt => opt.MapFrom(src => src.IsTopicMultiple?"true":"false"));        

        CreateMap<Topic, TopicListDto>()
            .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.TopicName))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TopicID));

        CreateMap<Topic, SupervisorTopicDto>()
            .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.TopicName))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TopicID));
    }
}

