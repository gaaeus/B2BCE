namespace API.Contracts.Companies;

public sealed record CompanyListResponse(
    IReadOnlyCollection<CompanyResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    int TotalPages,
    bool HasNext,
    bool HasPrevious
);
