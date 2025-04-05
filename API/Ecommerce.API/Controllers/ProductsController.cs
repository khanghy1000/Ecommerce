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
        [FromQuery] List<int>? subCategoryIds = null
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
                SubcategoryIds = subCategoryIds,
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

    [HttpPost]
    [Authorize(Roles = "Seller,Admin")]
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
    [Authorize(Roles = "Seller,Admin", Policy = "IsProductOwner")]
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

    [HttpDelete("{id}")]
    [Authorize(Roles = "Seller,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<Unit>> DeleteProduct(int id)
    {
        var result = await Mediator.Send(new DeleteProduct.Command { Id = id });
        return HandleResult(result);
    }

    [HttpPost("{id}/photos")]
    [Authorize(Roles = "Seller,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<ProductPhoto>> AddProductPhoto(int id, IFormFile file)
    {
        var result = await Mediator.Send(
            new AddProductPhoto.Command { ProductId = id, File = file }
        );
        return HandleResult(result);
    }

    [HttpDelete("{id}/photos")]
    [Authorize(Roles = "Seller,Admin", Policy = "IsProductOwner")]
    public async Task<ActionResult<Unit>> DeleteProductPhoto(int id, [FromQuery] string photoKey)
    {
        var result = await Mediator.Send(
            new DeleteProductPhoto.Command { ProductId = id, Key = photoKey }
        );
        return HandleResult(result);
    }

    [HttpPut("{id}/photos/order")]
    [Authorize(Roles = "Seller,Admin", Policy = "IsProductOwner")]
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
}
