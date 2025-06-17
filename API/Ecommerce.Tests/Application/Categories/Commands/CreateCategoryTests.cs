using AutoMapper;
using Ecommerce.Application.Categories.Commands;
using Ecommerce.Application.Categories.DTOs;
using Ecommerce.Tests.Application.Categories.Helpers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Categories.Commands;

public class CreateCategoryTests
{
    private readonly IMapper _mapper = CategoryTestHelper.GetMapper();
    private readonly TestAppDbContext _dbContext = CategoryTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task CreateCategory_ShouldAddCategoryToDatabase()
    {
        // Arrange
        var initialCategoryCount = await _dbContext.Categories.CountAsync();
        var command = new CreateCategory.Command
        {
            CreateCategoryRequestDto = new CreateCategoryRequestDto { Name = "Test Category" },
        };
        var handler = new CreateCategory.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Test Category");

        var updatedCategoryCount = await _dbContext.Categories.CountAsync();
        updatedCategoryCount.ShouldBe(initialCategoryCount + 1);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnCategoryWithCorrectId()
    {
        // Arrange
        var command = new CreateCategory.Command
        {
            CreateCategoryRequestDto = new CreateCategoryRequestDto { Name = "New Category" },
        };
        var handler = new CreateCategory.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();

        var storedCategory = await _dbContext.Categories.FindAsync(result.Value.Id);
        storedCategory.ShouldNotBeNull();
        storedCategory.Name.ShouldBe("New Category");
    }
}
