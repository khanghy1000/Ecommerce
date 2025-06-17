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

public class DeleteProductPhotoTests
{
    private readonly IMapper _mapper = ProductTestHelper.GetMapper();
    private readonly IUserAccessor _userAccessor = A.Fake<IUserAccessor>();
    private readonly IPhotoService _photoService = A.Fake<IPhotoService>();
    private readonly TestAppDbContext _dbContext = ProductTestHelper
        .GetDbContext()
        .GetAwaiter()
        .GetResult();

    [Fact]
    public async Task DeleteProductPhoto_ShouldReturnSuccess()
    {
        var product = await _dbContext
            .Products.Include(product => product.Photos)
            .FirstOrDefaultAsync();
        var photo = new ProductPhoto
        {
            Key = "photos/products/1/photo1.jpg",
            DisplayOrder = 1,
            ProductId = product!.Id,
        };
        product.Photos.Clear();
        product.Photos.Add(photo);
        _dbContext.ProductPhotos.Add(photo);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteProductPhoto.Command { Key = photo.Key, ProductId = product.Id };
        var handler = new DeleteProductPhoto.Handler(_dbContext, _photoService);

        var result = await handler.Handle(command, CancellationToken.None);
        var deletedPhoto = await _dbContext.ProductPhotos.FindAsync(photo.Key);

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
        deletedPhoto.ShouldBeNull();
    }
}
