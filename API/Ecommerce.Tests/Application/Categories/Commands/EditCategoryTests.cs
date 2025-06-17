using AutoMapper;
using Ecommerce.Application.Categories.Commands;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Categories.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Categories.Commands;

public class EditCategoryTests
{
    private readonly IMapper _mapper = CategoryTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CategoryTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task EditCategory_WithValidId_ShouldUpdateCategory()
    {
        // Arrange
        var category = await _dbContext.Categories.FirstAsync();
        var originalName = category.Name;
        var newName = "Updated Category Name";

        var command = new EditCategory.Command
        {
            Id = category.Id,
            EditCategoryRequestDto = new EditCategoryRequestDto { Name = newName },
        };
        var handler = new EditCategory.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe(newName);
        result.Value.Id.ShouldBe(category.Id);

        var updatedCategory = await _dbContext.Categories.FindAsync(category.Id);
        updatedCategory.ShouldNotBeNull();
        updatedCategory.Name.ShouldBe(newName);
        updatedCategory.Name.ShouldNotBe(originalName);
    }

    [Fact]
    public async Task EditCategory_WithInvalidId_ShouldReturnFailure()
    {
        // Arrange
        var invalidId = 9999;
        var command = new EditCategory.Command
        {
            Id = invalidId,
            EditCategoryRequestDto = new EditCategoryRequestDto { Name = "Test Name" },
        };
        var handler = new EditCategory.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe("Category not found");
    }
}
