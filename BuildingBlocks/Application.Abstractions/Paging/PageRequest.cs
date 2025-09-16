namespace BuildingBlocks.Application.Abstractions.Paging;

public sealed record PageRequest(int Page = 1, int PageSize = 20)
{
    public int Skip => (Page < 1 ? 0 : (Page - 1)) * (PageSize < 1 ? 20 : PageSize);
    public int Take => PageSize < 1 ? 20 : PageSize;
    public static PageRequest Default => new(1, 20);
}
