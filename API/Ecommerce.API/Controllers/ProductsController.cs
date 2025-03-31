using Ecommerce.Application.Core;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

public class ProductsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PagedList<ProductDto>>> ListProducts(
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
                CategoryIds = categoryIds,
            }
        );
        return HandleResult(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        var product = await Mediator.Send(new GetProductById.Query { Id = id });
        return HandleResult(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto product)
    {
        var result = await Mediator.Send(new CreateProduct.Command { ProductDto = product });
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> EditProduct(int id, EditProductDto product)
    {
        var result = await Mediator.Send(new EditProduct.Command { Id = id, ProductDto = product });
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeleteProduct(int id)
    {
        var result = await Mediator.Send(new DeleteProduct.Command { Id = id });
        return HandleResult(result);
    }
}
