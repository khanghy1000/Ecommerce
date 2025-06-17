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
    public async Task CreateCategory_ShouldCreateNewCategory()
    {
        // Arrange
        var command = new CreateCategory.Command
        {
            CreateCategoryRequestDto = new CreateCategoryRequestDto { Name = "Test Category" },
        };
        var handler = new CreateCategory.Handler(_dbContext, _mapper);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        var createdCategory = await _dbContext.Categories.FirstOrDefaultAsync(c =>
            c.Id == result.Value.Id
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        createdCategory.ShouldNotBeNull();
        createdCategory.Name.ShouldBe("Test Category");
        createdCategory.Id.ShouldBe(result.Value.Id);
    }
}
