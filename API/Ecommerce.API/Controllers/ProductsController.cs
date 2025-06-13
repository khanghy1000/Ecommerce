using Ecommerce.Application.Core;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

public class ProductsController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedList<ProductResponseDto>>> ListProducts(
        string? keyword,
        int pageSize = 20,
        int pageNumber = 1,
        string sortBy = "name",
        string sortDirection = "asc",
        int? categoryId = null,
        [FromQuery] List<int>? subCategoryIds = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool includeInactive = false,
        string? shopId = null
    )
    {
        var products = await Mediator.Send(
            new ListProducts.Query
            {
                Keyword = keyword,
                PageSize = pageSize,
                PageNumber = pageNumber,
                SortBy = sortBy.ToLower(),
                SortDirection = sortDirection.ToLower(),
                CategoryId = categoryId,
                SubcategoryIds = subCategoryIds,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                IncludeInactive = includeInactive,
                ShopId = shopId,
            }
        );
        return HandleResult(products);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductResponseDto>> GetProductById(int id)
    {
        var product = await Mediator.Send(new GetProductById.Query { Id = id });
        return HandleResult(product);
    }

    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<ActionResult<List<PopularProductResponseDto>>> GetPopularProducts()
    {
        var products = await Mediator.Send(new GetPopularProducts.Query());
        return HandleResult(products);
    }

    [HttpPost]
    [Authorize(Roles = "Shop")]
    public async Task<ActionResult<ProductResponseDto>> CreateProduct(
        CreateProductRequestDto createProductRequestDto
    )
    {
        var result = await Mediator.Send(
            new CreateProduct.Command { CreateProductRequestDto = createProductRequestDto }
        );
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Shop,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<ProductResponseDto>> EditProduct(
        int id,
        EditProductRequestDto editProductRequestDto
    )
    {
        var result = await Mediator.Send(
            new EditProduct.Command { Id = id, EditProductRequestDto = editProductRequestDto }
        );
        return HandleResult(result);
    }

    [HttpPut("{id}/active")]
    [Authorize(Roles = "Shop,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<Product>> SetProductActiveState(int id, bool isActive)
    {
        var result = await Mediator.Send(
            new SetProductActiveState.Command { ProductId = id, IsActive = isActive }
        );
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Shop,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<Unit>> DeleteProduct(int id)
    {
        var result = await Mediator.Send(new DeleteProduct.Command { Id = id });
        return HandleResult(result);
    }

    [HttpPost("{id}/photos")]
    [Authorize(Roles = "Shop,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<ProductPhoto>> AddProductPhoto(int id, IFormFile file)
    {
        var result = await Mediator.Send(
            new AddProductPhoto.Command { ProductId = id, File = file }
        );
        return HandleResult(result);
    }

    [HttpDelete("{id}/photos")]
    [Authorize(Roles = "Shop,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<Unit>> DeleteProductPhoto(int id, [FromQuery] string photoKey)
    {
        var result = await Mediator.Send(
            new DeleteProductPhoto.Command { ProductId = id, Key = photoKey }
        );
        return HandleResult(result);
    }

    [HttpPut("{id}/photos/order")]
    [Authorize(Roles = "Shop,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<Unit>> UpdateProductPhotoDisplayOrder(
        int id,
        List<UpdateProductPhotoDisplayOrderRequestDto> photoOrderRequestDto
    )
    {
        var result = await Mediator.Send(
            new UpdateProductPhotoDisplayOrder.Command
            {
                ProductId = id,
                PhotoOrders = photoOrderRequestDto,
            }
        );
        return HandleResult(result);
    }

    [HttpGet("{id}/discounts")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ProductDiscountResponseDto>>> ListProductDiscounts(int id)
    {
        var result = await Mediator.Send(new ListProductDiscounts.Query { ProductId = id });
        return HandleResult(result);
    }

    [HttpGet("{id}/discounts/{discountId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDiscountResponseDto>> GetProductDiscountById(
        int id,
        int discountId
    )
    {
        var result = await Mediator.Send(
            new GetProductDiscountById.Query { ProductId = id, DiscountId = discountId }
        );
        return HandleResult(result);
    }

    [HttpPost("{id}/discounts")]
    [Authorize(Roles = "Shop,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<ProductDiscountResponseDto>> AddProductDiscount(
        int id,
        AddProductDiscountRequestDto discountDto
    )
    {
        var result = await Mediator.Send(
            new AddProductDiscount.Command { ProductId = id, DiscountDto = discountDto }
        );
        return HandleResult(result);
    }

    [HttpPut("{id}/discounts/{discountId}")]
    [Authorize(Roles = "Shop,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<ProductDiscountResponseDto>> EditProductDiscount(
        int id,
        int discountId,
        EditProductDiscountRequestDto discountDto
    )
    {
        var result = await Mediator.Send(
            new EditProductDiscount.Command
            {
                ProductId = id,
                DiscountId = discountId,
                DiscountDto = discountDto,
            }
        );
        return HandleResult(result);
    }

    [HttpDelete("{id}/discounts/{discountId}")]
    [Authorize(Roles = "Shop,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<Unit>> DeleteProductDiscount(int id, int discountId)
    {
        var result = await Mediator.Send(
            new DeleteProductDiscount.Command { ProductId = id, DiscountId = discountId }
        );
        return HandleResult(result);
    }
}
