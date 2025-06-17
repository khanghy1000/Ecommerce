using AutoMapper;
using Ecommerce.Application.Categories.Commands;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Tests.Application.Categories.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Categories.Commands;

public class CreateSubcategoryTests
{
    private readonly IMapper _mapper = CategoryTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CategoryTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task CreateSubcategory_WithValidCategoryId_ShouldAddSubcategory()
    {
        // Arrange
        var category = await _dbContext.Categories.FirstAsync();
        var initialSubcategoryCount = await _dbContext.Subcategories.CountAsync(s =>
            s.CategoryId == category.Id
        );

        var command = new CreateSubcategory.Command
        {
            CreateSubcategoryRequestDto = new CreateSubcategoryRequestDto
            {
                Name = "Test Subcategory",
                CategoryId = category.Id,
            },
        };
        var handler = new CreateSubcategory.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Test Subcategory");
        result.Value.CategoryId.ShouldBe(category.Id);

        var updatedSubcategoryCount = await _dbContext.Subcategories.CountAsync(s =>
            s.CategoryId == category.Id
        );
        updatedSubcategoryCount.ShouldBe(initialSubcategoryCount + 1);
    }

    [Fact]
    public async Task CreateSubcategory_WithInvalidCategoryId_ShouldReturnFailure()
    {
        // Arrange
        var invalidCategoryId = 9999;
        var command = new CreateSubcategory.Command
        {
            CreateSubcategoryRequestDto = new CreateSubcategoryRequestDto
            {
                Name = "Test Subcategory",
                CategoryId = invalidCategoryId,
            },
        };
        var handler = new CreateSubcategory.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Category not found");
    }
}
