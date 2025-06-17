using AutoMapper;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Categories.Queries;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Categories.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Categories.Queries;

public class GetCategoryByIdTests
{
    private readonly IMapper _mapper = CategoryTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CategoryTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task GetCategoryById_WithValidId_ShouldReturnCategoryAndSubcategories()
    {
        // Arrange
        var existingCategory = await _dbContext
            .Categories.Include(c => c.Subcategories)
            .Where(c => c.Subcategories.Any())
            .FirstAsync();

        var query = new GetCategoryById.Query { Id = existingCategory.Id };
        var handler = new GetCategoryById.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(existingCategory.Id);
        result.Value.Name.ShouldBe(existingCategory.Name);

        result.Value.Subcategories.ShouldNotBeEmpty();
        result.Value.Subcategories.Count.ShouldBe(existingCategory.Subcategories.Count);
    }

    [Fact]
    public async Task GetCategoryById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = 9999;
        var query = new GetCategoryById.Query { Id = invalidId };
        var handler = new GetCategoryById.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Category not found");
        result.Code.ShouldBe(404);
    }
}
