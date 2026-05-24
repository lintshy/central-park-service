namespace CentralPark.Shared;

public sealed class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;

    private PagedList(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PagedList<T> Create(IEnumerable<T> source, int page, int pageSize)
    {
        var items = source.ToList();
        var count = items.Count;
        var paged = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PagedList<T>(paged.AsReadOnly(), page, pageSize, count);
    }
}
