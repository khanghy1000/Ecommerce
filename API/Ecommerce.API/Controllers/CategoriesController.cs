using Ecommerce.Application.Categories.Commands;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

public class CategoriesController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryResponseDto>>> GetCategories()
    {
        var categories = await Mediator.Send(new ListCategories.Query());
        return HandleResult(categories);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryResponseDto>> GetCategoryById(int id)
    {
        var category = await Mediator.Send(new GetCategoryById.Query { Id = id });
        return HandleResult(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryWithoutChildResponseDto>> CreateCategory(
        CreateCategoryRequestDto createCategoryRequestDto
    )
    {
        var result = await Mediator.Send(
            new CreateCategory.Command { CreateCategoryRequestDto = createCategoryRequestDto }
        );
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryWithoutChildResponseDto>> EditCategory(
        int id,
        EditCategoryRequestDto editCategoryRequestDto
    )
    {
        var result = await Mediator.Send(
            new EditCategory.Command { Id = id, EditCategoryRequestDto = editCategoryRequestDto }
        );
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Unit>> DeleteCategory(int id)
    {
        var result = await Mediator.Send(new DeleteCategory.Command { Id = id });
        return HandleResult(result);
    }

    [HttpGet("subcategories/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<SubcategoryResponseDto>> GetSubcategoryById(int id)
    {
        var subcategory = await Mediator.Send(new GetSubcategoryById.Query { Id = id });
        return HandleResult(subcategory);
    }

    [HttpPost("subcategories")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubcategoryResponseDto>> CreateSubcategory(
        CreateSubcategoryRequestDto createSubcategoryRequestDto
    )
    {
        var result = await Mediator.Send(
            new CreateSubcategory.Command
            {
                CreateSubcategoryRequestDto = createSubcategoryRequestDto,
            }
        );
        return HandleResult(result);
    }

    [HttpPut("subcategories/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubcategoryResponseDto>> EditSubcategory(
        int id,
        EditSubcategoryRequestDto editSubcategoryRequestDto
    )
    {
        var result = await Mediator.Send(
            new EditSubcategory.Command
            {
                Id = id,
                EditSubcategoryRequestDto = editSubcategoryRequestDto,
            }
        );
        return HandleResult(result);
    }

    [HttpDelete("subcategories/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Unit>> DeleteSubcategory(int id)
    {
        var result = await Mediator.Send(new DeleteSubcategory.Command { Id = id });
        return HandleResult(result);
    }
}
