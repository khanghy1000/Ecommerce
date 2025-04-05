using Ecommerce.Application.Core;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

public class ProductsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PagedList<ProductResponseDto>>> ListProducts(
        string? keyword,
        int pageSize = 20,
        int pageNumber = 1,
        string sortBy = "name",
        string sortDirection = "asc",
        [FromQuery] List<int>? categoryIds = null
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
                SubcategoryIds = categoryIds,
            }
        );
        return HandleResult(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> GetProductById(int id)
    {
        var product = await Mediator.Send(new GetProductById.Query { Id = id });
        return HandleResult(product);
    }

    [HttpPost]
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
    public async Task<ActionResult<Unit>> DeleteProduct(int id)
    {
        var result = await Mediator.Send(new DeleteProduct.Command { Id = id });
        return HandleResult(result);
    }

    [HttpPost("{productId}/photos")]
    public async Task<ActionResult<ProductPhoto>> AddProductPhoto(int productId, IFormFile file)
    {
        var result = await Mediator.Send(
            new AddProductPhoto.Command { ProductId = productId, File = file }
        );
        return HandleResult(result);
    }

    [HttpDelete("{productId}/photos")]
    public async Task<ActionResult<Unit>> DeleteProductPhoto(
        int productId,
        [FromQuery] string photoKey
    )
    {
        var result = await Mediator.Send(
            new DeleteProductPhoto.Command { ProductId = productId, Key = photoKey }
        );
        return HandleResult(result);
    }

    [HttpPut("{productId}/photos/order")]
    public async Task<ActionResult<Unit>> UpdateProductPhotoDisplayOrder(
        int productId,
        List<UpdateProductPhotoDisplayOrderRequestDto> photoOrderRequestDto
    )
    {
        var result = await Mediator.Send(
            new UpdateProductPhotoDisplayOrder.Command
            {
                ProductId = productId,
                PhotoOrders = photoOrderRequestDto,
            }
        );
        return HandleResult(result);
    }
}
