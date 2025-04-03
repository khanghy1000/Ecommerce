using AutoMapper;
using Ecommerce.Application.CartItems.DTOs;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Locations.DTOs;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Domain;

namespace Ecommerce.Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        string? currentUserId = null;
        // Product mappings
        CreateMap<Product, Product>();
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.ShopName, opt => opt.MapFrom(src => src.Shop.DisplayName))
            .ForMember(dest => dest.ShopImageUrl, opt => opt.MapFrom(src => src.Shop.ImageUrl));
        CreateMap<CreateProductDto, Product>();
        CreateMap<EditProductDto, Product>();

        // Category mappings
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<EditCategoryDto, Category>();
        CreateMap<Category, CategoryWithoutChildDto>();

        // Subcategory mappings
        CreateMap<Subcategory, SubcategoryDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
        CreateMap<Subcategory, SubcategoryNameDto>();

        // CartItem mappings
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.MaxQuantity, opt => opt.MapFrom(src => src.Product.Quantity))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Product.RegularPrice))
            .ForMember(
                dest => dest.DiscountPrice,
                opt => opt.MapFrom(src => src.Product.DiscountPrice)
            )
            .ForMember(
                dest => dest.Subtotal,
                opt =>
                    opt.MapFrom(src =>
                        (src.Product.DiscountPrice ?? src.Product.RegularPrice) * src.Quantity
                    )
            )
            .ForMember(
                dest => dest.ProductImageUrl,
                opt =>
                    opt.MapFrom(src =>
                        src.Product.Photos.OrderBy(p => p.DisplayOrder)
                            .Select(p => p.Key)
                            .FirstOrDefault() ?? ""
                    )
            )
            .ForMember(dest => dest.ShopId, opt => opt.MapFrom(src => src.Product.ShopId))
            .ForMember(
                dest => dest.ShopName,
                opt => opt.MapFrom(src => src.Product.Shop.DisplayName)
            );

        CreateMap<Province, ProvinceDto>();
        CreateMap<District, DistrictDto>()
            .ForMember(dest => dest.ProvinceName, opt => opt.MapFrom(src => src.Province.Name));
        CreateMap<Ward, WardDto>()
            .ForMember(dest => dest.DistrictName, opt => opt.MapFrom(src => src.District.Name));
    }
}
