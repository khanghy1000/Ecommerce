namespace Ecommerce.Application.Core;

public class Result<T>
{
    public T? Value { get; set; }
    public string? Error { get; set; }
    public int Code { get; set; }
    public bool IsSuccess => string.IsNullOrEmpty(Error);

    public static Result<T> Success(T value) => new() { Value = value, Code = 200 };

    public static Result<T> Failure(string error, int code) => new() { Error = error, Code = code };
}
