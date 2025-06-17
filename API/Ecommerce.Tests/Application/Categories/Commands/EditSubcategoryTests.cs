using AutoMapper;
using Ecommerce.Application.Categories.Commands;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Categories.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Categories.Commands;

public class EditSubcategoryTests
{
    private readonly IMapper _mapper = CategoryTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CategoryTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task EditSubcategory_WithValidData_ShouldUpdateSubcategory()
    {
        // Arrange
        var subcategory = await _dbContext.Subcategories.FirstAsync();
        var originalName = subcategory.Name;
        var newName = "Updated Subcategory Name";

        var command = new EditSubcategory.Command
        {
            Id = subcategory.Id,
            EditSubcategoryRequestDto = new EditSubcategoryRequestDto
            {
                Name = newName,
                CategoryId = subcategory.CategoryId, // Keep the same category
            },
        };
        var handler = new EditSubcategory.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe(newName);
        result.Value.Id.ShouldBe(subcategory.Id);
        result.Value.CategoryId.ShouldBe(subcategory.CategoryId);

        var updatedSubcategory = await _dbContext.Subcategories.FindAsync(subcategory.Id);
        updatedSubcategory.ShouldNotBeNull();
        updatedSubcategory.Name.ShouldBe(newName);
        updatedSubcategory.Name.ShouldNotBe(originalName);
    }

    [Fact]
    public async Task EditSubcategory_WithInvalidCategoryId_ShouldReturnFailure()
    {
        // Arrange
        var subcategory = await _dbContext.Subcategories.FirstAsync();
        var invalidCategoryId = 9999;

        var command = new EditSubcategory.Command
        {
            Id = subcategory.Id,
            EditSubcategoryRequestDto = new EditSubcategoryRequestDto
            {
                Name = "Updated Name",
                CategoryId = invalidCategoryId,
            },
        };
        var handler = new EditSubcategory.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Target category not found");
        result.Code.ShouldBe(400);
    }

    [Fact]
    public async Task EditSubcategory_MovingToAnotherCategory_ShouldUpdateCategoryId()
    {
        // Arrange
        var subcategory = await _dbContext.Subcategories.FirstAsync();
        var originalCategoryId = subcategory.CategoryId;

        // Find another category different from the current one
        var newCategory = await _dbContext
            .Categories.Where(c => c.Id != originalCategoryId)
            .FirstAsync();

        var command = new EditSubcategory.Command
        {
            Id = subcategory.Id,
            EditSubcategoryRequestDto = new EditSubcategoryRequestDto
            {
                Name = "Moved Subcategory",
                CategoryId = newCategory.Id,
            },
        };
        var handler = new EditSubcategory.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.CategoryId.ShouldBe(newCategory.Id);
        result.Value.CategoryId.ShouldNotBe(originalCategoryId);

        var updatedSubcategory = await _dbContext.Subcategories.FindAsync(subcategory.Id);
        updatedSubcategory.ShouldNotBeNull();
        updatedSubcategory.CategoryId.ShouldBe(newCategory.Id);
    }
}
