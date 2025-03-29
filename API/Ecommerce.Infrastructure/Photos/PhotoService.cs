using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Photos;

public class PhotoService : IPhotoService
{
    private readonly AmazonS3Client _s3Client;
    private readonly IOptions<S3Settings> _config;

    public PhotoService(IOptions<S3Settings> config)
    {
        _config = config;
        var accessKey = _config.Value.AccessKey;
        var secretKey = _config.Value.SecretKey;
        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        _s3Client = new AmazonS3Client(
            credentials,
            new AmazonS3Config
            {
                ServiceURL = _config.Value.ServiceUrl,
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            }
        );
    }

    public async Task<S3UploadResult?> UploadPhoto(IFormFile file, string prefix = "photos")
    {
        if (file.Length <= 0)
            return null;

        var stream = file.OpenReadStream();
        var key = $"{prefix}/{Guid.NewGuid()}-{file.FileName}";
        var request = new PutObjectRequest
        {
            Key = key,
            InputStream = stream,
            BucketName = _config.Value.BucketName,
            DisablePayloadSigning = true,
        };
        var response = await _s3Client.PutObjectAsync(request);
        return new S3UploadResult { Key = request.Key, PutObjectResponse = response };
    }

    public Task<DeleteObjectResponse> DeletePhoto(string key)
    {
        var request = new DeleteObjectRequest { BucketName = _config.Value.BucketName, Key = key };
        return _s3Client.DeleteObjectAsync(request);
    }

    public Task<DeleteObjectsResponse> DeletePhotos(string[] keys)
    {
        var request = new DeleteObjectsRequest { BucketName = _config.Value.BucketName };
        foreach (var key in keys)
        {
            request.AddKey(key);
        }
        return _s3Client.DeleteObjectsAsync(request);
    }
}
