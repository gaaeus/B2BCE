namespace BuildingBlocks.Application.Abstractions.Paging;

public sealed record PageResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, long TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
