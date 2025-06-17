using AutoMapper;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Application.Categories.Queries;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Categories.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Categories.Queries;

public class GetSubcategoryByIdTests
{
    private readonly IMapper _mapper = CategoryTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CategoryTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task GetSubcategoryById_WithValidId_ShouldReturnSubcategory()
    {
        // Arrange
        var existingSubcategory = await _dbContext.Subcategories.FirstAsync();
        var query = new GetSubcategoryById.Query { Id = existingSubcategory.Id };
        var handler = new GetSubcategoryById.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(existingSubcategory.Id);
        result.Value.Name.ShouldBe(existingSubcategory.Name);
        result.Value.CategoryId.ShouldBe(existingSubcategory.CategoryId);
    }

    [Fact]
    public async Task GetSubcategoryById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = 9999;
        var query = new GetSubcategoryById.Query { Id = invalidId };
        var handler = new GetSubcategoryById.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Subcategory not found");
        result.Code.ShouldBe(404);
    }

    [Fact]
    public async Task GetSubcategoryById_ShouldReturnCategoryInfo()
    {
        // Arrange
        var existingSubcategory = await _dbContext
            .Subcategories.Include(s => s.Category)
            .FirstAsync();

        var query = new GetSubcategoryById.Query { Id = existingSubcategory.Id };
        var handler = new GetSubcategoryById.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.CategoryId.ShouldBe(existingSubcategory.CategoryId);
        result.Value.CategoryName.ShouldBe(existingSubcategory.Category.Name);
    }
}
