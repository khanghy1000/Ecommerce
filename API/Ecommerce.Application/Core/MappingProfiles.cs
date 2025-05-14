using AutoMapper;
using Ecommerce.Application.CartItems.DTOs;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Coupons.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Locations.DTOs;
using Ecommerce.Application.Payments.DTOs;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Application.Reviews.DTOs;
using Ecommerce.Application.SalesOrders.DTOs;
using Ecommerce.Application.Users.DTOs;
using Ecommerce.Domain;

namespace Ecommerce.Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        string? currentUserId = null;
        // Product mappings
        CreateMap<Product, Product>();
        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.ShopName, opt => opt.MapFrom(src => src.Shop.DisplayName))
            .ForMember(dest => dest.ShopImageUrl, opt => opt.MapFrom(src => src.Shop.ImageUrl))
            .ForMember(
                dest => dest.DiscountPrice,
                opt =>
                    opt.MapFrom(src =>
                        src.Discounts.Where(d =>
                                d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                            )
                            .OrderBy(d => d.DiscountPrice)
                            .Select(d => (decimal?)d.DiscountPrice)
                            .FirstOrDefault()
                    )
            );
        CreateMap<CreateProductRequestDto, Product>();
        CreateMap<EditProductRequestDto, Product>();

        // ProductDiscount mappings
        CreateMap<ProductDiscount, ProductDiscountResponseDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(
                dest => dest.RegularPrice,
                opt => opt.MapFrom(src => src.Product.RegularPrice)
            );

        // Category mappings
        CreateMap<Category, CategoryResponseDto>();
        CreateMap<CreateCategoryRequestDto, Category>();
        CreateMap<EditCategoryRequestDto, Category>();
        CreateMap<Category, CategoryWithoutChildResponseDto>();

        // Subcategory mappings
        CreateMap<Subcategory, SubcategoryResponseDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
        CreateMap<Subcategory, SubcategoryIdNameResponseDto>();

        // CartItem mappings
        CreateMap<CartItem, CartItemResponseDto>()
            .ForMember(dest => dest.MaxQuantity, opt => opt.MapFrom(src => src.Product.Quantity))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Product.RegularPrice))
            .ForMember(
                dest => dest.DiscountPrice,
                opt =>
                    opt.MapFrom(src =>
                        src.Product.Discounts.Where(d =>
                                d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                            )
                            .OrderBy(d => d.DiscountPrice)
                            .Select(d => (decimal?)d.DiscountPrice)
                            .FirstOrDefault()
                    )
            )
            .ForMember(
                dest => dest.Subtotal,
                opt =>
                    opt.MapFrom(src =>
                        (
                            src.Product.Discounts.Where(d =>
                                    d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                                )
                                .OrderBy(d => d.DiscountPrice)
                                .Select(d => (decimal?)d.DiscountPrice)
                                .FirstOrDefault() ?? src.Product.RegularPrice
                        ) * src.Quantity
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
            )
            .ForMember(
                dest => dest.ShopImageUrl,
                opt => opt.MapFrom(src => src.Product.Shop.ImageUrl)
            );

        CreateMap<Province, ProvinceResponseDto>();
        CreateMap<District, DistrictResponseDto>()
            .ForMember(dest => dest.ProvinceName, opt => opt.MapFrom(src => src.Province.Name));
        CreateMap<Ward, WardResponseDto>()
            .ForMember(dest => dest.DistrictName, opt => opt.MapFrom(src => src.District.Name));

        CreateMap<UserAddress, UserAddressResponseDto>()
            .ForMember(dest => dest.DistrictId, opt => opt.MapFrom(src => src.Ward.DistrictId))
            .ForMember(
                dest => dest.ProvinceId,
                opt => opt.MapFrom(src => src.Ward.District.ProvinceId)
            )
            .ForMember(dest => dest.WardName, opt => opt.MapFrom(src => src.Ward.Name))
            .ForMember(dest => dest.DistrictName, opt => opt.MapFrom(src => src.Ward.District.Name))
            .ForMember(
                dest => dest.ProvinceName,
                opt => opt.MapFrom(src => src.Ward.District.Province.Name)
            );

        CreateMap<CartItem, CreateShippingRequestItem>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Product.Id))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(
                dest => dest.Price,
                opt =>
                    opt.MapFrom(src =>
                        (int)(
                            src.Product.Discounts.Where(d =>
                                    d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                                )
                                .OrderBy(d => d.DiscountPrice)
                                .Select(d => (decimal?)d.DiscountPrice)
                                .FirstOrDefault() ?? src.Product.RegularPrice
                        )
                    )
            )
            .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Product.Length))
            .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Product.Width))
            .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Product.Height))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Product.Weight));

        CreateMap<OrderProduct, CreateShippingRequestItem>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Product.Id))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(
                dest => dest.Price,
                opt =>
                    opt.MapFrom(src =>
                        (int)(
                            src.Product.Discounts.Where(d =>
                                    d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                                )
                                .OrderBy(d => d.DiscountPrice)
                                .Select(d => (decimal?)d.DiscountPrice)
                                .FirstOrDefault() ?? src.Product.RegularPrice
                        )
                    )
            )
            .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Product.Length))
            .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Product.Width))
            .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Product.Height))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Product.Weight));

        CreateMap<SalesOrder, SalesOrderResponseDto>()
            .ForMember(
                dest => dest.ShippingDistrictId,
                opt => opt.MapFrom(src => src.ShippingWard.DistrictId)
            )
            .ForMember(
                dest => dest.ShippingProvinceId,
                opt => opt.MapFrom(src => src.ShippingWard.District.ProvinceId)
            )
            .ForMember(
                dest => dest.ShippingWardName,
                opt => opt.MapFrom(src => src.ShippingWard.Name)
            )
            .ForMember(
                dest => dest.ShippingDistrictName,
                opt => opt.MapFrom(src => src.ShippingWard.District.Name)
            )
            .ForMember(
                dest => dest.ShippingProvinceName,
                opt => opt.MapFrom(src => src.ShippingWard.District.Province.Name)
            )
            .ForMember(
                dest => dest.ProductCouponCode,
                opt =>
                    opt.MapFrom(src =>
                        src.Coupons.Where(c => c.Type == CouponType.Product)
                            .Select(c => c.Code)
                            .FirstOrDefault()
                    )
            )
            .ForMember(
                dest => dest.ShippingCouponCode,
                opt =>
                    opt.MapFrom(src =>
                        src.Coupons.Where(c => c.Type == CouponType.Shipping)
                            .Select(c => c.Code)
                            .FirstOrDefault()
                    )
            );

        CreateMap<OrderProduct, OrderProductResponseDto>();

        // In the MappingProfiles constructor:
        CreateMap<VNPAY.NET.Models.PaymentResult, Payment>()
            .ForMember(
                dest => dest.ResponseCode,
                opt => opt.MapFrom(src => src.PaymentResponse.Code)
            )
            .ForMember(
                dest => dest.ResponseDescription,
                opt => opt.MapFrom(src => src.PaymentResponse.Description)
            )
            .ForMember(
                dest => dest.TransactionCode,
                opt => opt.MapFrom(src => src.TransactionStatus.Code)
            )
            .ForMember(
                dest => dest.TransactionDescription,
                opt => opt.MapFrom(src => src.TransactionStatus.Description)
            )
            .ForMember(dest => dest.BankCode, opt => opt.MapFrom(src => src.BankingInfor.BankCode))
            .ForMember(
                dest => dest.BankTransactionId,
                opt => opt.MapFrom(src => src.BankingInfor.BankTransactionId)
            );

        CreateMap<PopularProduct, PopularProductResponseDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(
                dest => dest.RegularPrice,
                opt => opt.MapFrom(src => src.Product.RegularPrice)
            )
            .ForMember(
                dest => dest.DiscountPrice,
                opt =>
                    opt.MapFrom(src =>
                        src.Product.Discounts.Where(d =>
                                d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow
                            )
                            .OrderBy(d => d.DiscountPrice)
                            .Select(d => (decimal?)d.DiscountPrice)
                            .FirstOrDefault()
                    )
            )
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Product.Quantity))
            .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Product.Active))
            .ForMember(dest => dest.ShopId, opt => opt.MapFrom(src => src.Product.ShopId))
            .ForMember(
                dest => dest.ShopName,
                opt => opt.MapFrom(src => src.Product.Shop.DisplayName)
            )
            .ForMember(
                dest => dest.ShopImageUrl,
                opt => opt.MapFrom(src => src.Product.Shop.ImageUrl)
            )
            .ForMember(
                dest => dest.Subcategories,
                opt => opt.MapFrom(src => src.Product.Subcategories)
            )
            .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Product.Photos));

        // Review mappings
        CreateMap<ProductReview, ReviewResponseDto>();

        // Coupon mappings
        CreateMap<Coupon, CouponResponseDto>();

        CreateMap<CreateCouponRequestDto, Coupon>()
            .ForMember(dest => dest.UsedCount, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.Categories, opt => opt.Ignore());

        CreateMap<EditCouponRequestDto, Coupon>()
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.UsedCount, opt => opt.Ignore())
            .ForMember(dest => dest.Categories, opt => opt.Ignore());

        // Payment mappings
        CreateMap<Payment, PaymentResponseDto>();
    }
}
