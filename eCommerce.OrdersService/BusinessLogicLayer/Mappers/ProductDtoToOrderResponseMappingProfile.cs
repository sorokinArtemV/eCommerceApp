using AutoMapper;
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Mappers;

public sealed class ProductDtoToOrderResponseMappingProfile : Profile
{
    public ProductDtoToOrderResponseMappingProfile()
    {
        CreateMap<ProductDto, OrderItemResponse>()
          .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
          .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
    }
}
