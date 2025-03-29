namespace Ecommerce.Infrastructure.Photos;

public class S3Settings
{
    public required string BucketName { get; set; }
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public required string ServiceUrl { get; set; }
}
