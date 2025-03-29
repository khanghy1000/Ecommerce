using Amazon.S3.Model;

namespace Ecommerce.Application.Core;

public class S3UploadResult
{
    public required string Key { get; set; }
    public required PutObjectResponse PutObjectResponse { get; set; }
}
