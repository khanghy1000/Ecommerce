namespace Ecommerce.Application.Core;

public class PagedList<T>
{
    public required int TotalCount { get; set; }
    public required int PageSize { get; set; }
    public required int PageNumber { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public required List<T> Items { get; set; } = [];
}
