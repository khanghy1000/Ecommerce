using Amazon.S3.Model;
using AutoMapper;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.DTOs;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Domain;
using Ecommerce.Tests.Application.Products.Helpers;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Ecommerce.Tests.Application.Products.Commands;

public class AddProductPhotoTests
{
    private readonly IMapper _mapper = ProductTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly IPhotoService _photoService = A.Fake<IPhotoService>();
    private readonly TestAppDbContext _dbContext = ProductTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task AddProductPhoto_ShouldReturnSuccess()
    {
        var product = await _dbContext
            .Products.Include(product => product.Photos)
            .FirstOrDefaultAsync();
        var file = A.Fake<IFormFile>();
        A.CallTo(() => _photoService.UploadPhoto(file, A<string>._))
            .Returns(
                new S3UploadResult
                {
                    Key = $"photos/products/{product!.Id}/{Guid.NewGuid().ToString()}-file.jpg",
                    PutObjectResponse = new PutObjectResponse
                    {
                        HttpStatusCode = System.Net.HttpStatusCode.OK,
                    },
                }
            );

        var command = new AddProductPhoto.Command { ProductId = product.Id, File = file };
        var handler = new AddProductPhoto.Handler(_dbContext, _photoService);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
        result.Value.ShouldNotBeNull();
        product.Photos.ShouldContain(result.Value);
    }
}
