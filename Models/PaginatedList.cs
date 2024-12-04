using Microsoft.EntityFrameworkCore;

public class PaginatedList<T>(IEnumerable<T> items, int count, int pageIndex, int pageSize) : List<T> (items)
{
    public int PageIndex { get; private set; } = pageIndex;
    public int TotalPages { get; private set; } = (int)Math.Ceiling(count / (double)pageSize);


    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}

public static class PaginatedListExtensions
{
    public static PaginatedList<T> ToPaginatedList<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}