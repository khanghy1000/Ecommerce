using AutoMapper;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Domain;

namespace Ecommerce.Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        string? currentUserId = null;
        CreateMap<Product, Product>();
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.ShopName, opt => opt.MapFrom(src => src.Shop.DisplayName))
            .ForMember(dest => dest.ShopImageUrl, opt => opt.MapFrom(src => src.Shop.ImageUrl));
        CreateMap<CreateProductDto, Product>();
        CreateMap<EditProductDto, Product>();
    }
}
