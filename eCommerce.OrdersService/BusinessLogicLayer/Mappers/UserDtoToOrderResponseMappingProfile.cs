using AutoMapper;
using BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.Mappers;

public sealed class UserDtoToOrderResponseMappingProfile : Profile
{
    public UserDtoToOrderResponseMappingProfile()
    {
        CreateMap<UserDto, OrderResponse>()
            .ForMember(dest => dest.UserPersonName, opt => opt.MapFrom(src => src.PersonName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
    }
}
