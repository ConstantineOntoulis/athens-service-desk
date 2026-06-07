using AthensServiceDesk.Application.DTOs.ServiceRequests;

namespace AthensServiceDesk.Application.Rules;

public static class QueryRules
{
    public const int DefaultPage = 1;

    public const int DefaultPageSize = 10;

    public const int MaxPageSize = 50;

    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "createdAt",
        "title",
        "status",
        "priority"
    };

    public static ServiceRequestQuery Normalize(ServiceRequestQuery? query)
    {
        query ??= new ServiceRequestQuery();

        return new ServiceRequestQuery
        {
            Page = NormalizePage(query.Page),
            PageSize = NormalizePageSize(query.PageSize),
            Search = NormalizeSearch(query.Search),
            Status = query.Status,
            Priority = query.Priority,
            DepartmentId = NormalizeOptionalId(query.DepartmentId),
            ServiceCategoryId = NormalizeOptionalId(query.ServiceCategoryId),
            SortBy = NormalizeSortBy(query.SortBy),
            SortDirection = NormalizeSortDirection(query.SortDirection)
        };
    }

    public static int NormalizePage(int page)
    {
        return page < 1 ? DefaultPage : page;
    }

    public static int NormalizePageSize(int pageSize)
    {
        if (pageSize < 1)
        {
            return DefaultPageSize;
        }

        return Math.Min(pageSize, MaxPageSize);
    }

    public static string? NormalizeSearch(string? search)
    {
        string? trimmedSearch = search?.Trim();

        return string.IsNullOrWhiteSpace(trimmedSearch)
            ? null
            : trimmedSearch;
    }

    public static int? NormalizeOptionalId(int? id)
    {
        return id is null or < 1 ? null : id;
    }

    public static string NormalizeSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return "createdAt";
        }

        string normalizedSortBy = sortBy.Trim();

        return AllowedSortFields.Contains(normalizedSortBy)
            ? normalizedSortBy
            : "createdAt";
    }

    public static string NormalizeSortDirection(string? sortDirection)
    {
        return string.Equals(sortDirection?.Trim(), "asc", StringComparison.OrdinalIgnoreCase)
            ? "asc"
            : "desc";
    }
}