using Amazon.S3.Model;
using Ecommerce.Application.Core;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Application.Interfaces;

public interface IPhotoService
{
    Task<S3UploadResult?> UploadPhoto(IFormFile file, string prefix = "photos");
    Task<DeleteObjectResponse> DeletePhoto(string key);
    Task<DeleteObjectsResponse> DeletePhotos(string[] keys);
}
