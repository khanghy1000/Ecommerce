using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers;

public class ProductsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts()
    {
        var products = await Mediator.Send(new ListProducts.Query());
        return HandleResult(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        var product = await Mediator.Send(new GetProductById.Query { Id = id });
        return HandleResult(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductDto product)
    {
        var result = await Mediator.Send(new CreateProduct.Command { ProductDto = product });
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> EditProduct(int id, EditProductDto product)
    {
        var result = await Mediator.Send(new EditProduct.Command { Id = id, ProductDto = product });
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await Mediator.Send(new DeleteProduct.Command { Id = id });
        return HandleResult(result);
    }
}
