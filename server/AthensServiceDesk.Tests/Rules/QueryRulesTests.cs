using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Application.Rules;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Tests.Rules;

public class QueryRulesTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1, 1)]
    [InlineData(3, 3)]

    public void NormalizePage_ShouldReturnValidPage(int input, int expected)
    {
        int result = QueryRules.NormalizePage(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-10, 10)]
    [InlineData(1, 1)]
    [InlineData(25, 25)]
    [InlineData(999, 50)]

    public void NormalizePageSize_ShouldReturnValidPageSize(int input, int expected)
    {
        int result = QueryRules.NormalizePageSize(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("  streetlight  ", "streetlight")]
    [InlineData("road damage", "road damage")]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    public void NormalizeSearch_ShouldTrimSearchOrReturnNull(string? input, string? expected)
    {
        string? result = QueryRules.NormalizeSearch(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(0, null)]
    [InlineData(-3, null)]
    [InlineData(1, 1)]
    [InlineData(7, 7)]
    public void NormalizeOptionalId_ShouldReturnOnlyPositiveIds(int? input, int? expected)
    {
        int? result = QueryRules.NormalizeOptionalId(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "createdAt")]
    [InlineData("", "createdAt")]
    [InlineData("   ", "createdAt")]
    [InlineData("banana", "createdAt")]
    [InlineData("createdAt", "createdAt")]
    [InlineData("title", "title")]
    [InlineData("status", "status")]
    [InlineData("priority", "priority")]
    public void NormalizeSortBy_ShouldReturnAllowedSortFieldOrDefault(string? input, string expected)
    {
        string result = QueryRules.NormalizeSortBy(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "desc")]
    [InlineData("", "desc")]
    [InlineData("banana", "desc")]
    [InlineData("desc", "desc")]
    [InlineData("asc", "asc")]
    [InlineData(" ASC ", "asc")]
    public void NormalizeSortDirection_ShouldReturnAscOnlyWhenAscIsProvided(string? input, string expected)
    {
        string result = QueryRules.NormalizeSortDirection(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Normalize_ShouldReturnSafeQuery_WhenQueryIsNull()
    {
        ServiceRequestQuery result = QueryRules.Normalize(null);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Null(result.Search);
        Assert.Null(result.Status);
        Assert.Null(result.Priority);
        Assert.Null(result.DepartmentId);
        Assert.Null(result.ServiceCategoryId);
        Assert.Equal("createdAt", result.SortBy);
        Assert.Equal("desc", result.SortDirection);
    }

    [Fact]
    public void Normalize_ShouldNormalizeAllQueryValues()
    {
        var query = new ServiceRequestQuery
        {
            Page = -4,
            PageSize = 999,
            Search = "  road damage  ",
            Status = ServiceRequestStatus.Submitted,
            Priority = ServicePriority.High,
            DepartmentId = -2,
            ServiceCategoryId = 3,
            SortBy = "banana",
            SortDirection = "asc"
        };

        ServiceRequestQuery result = QueryRules.Normalize(query);

        Assert.Equal(1, result.Page);
        Assert.Equal(50, result.PageSize);
        Assert.Equal("road damage", result.Search);
        Assert.Equal(ServiceRequestStatus.Submitted, result.Status);
        Assert.Equal(ServicePriority.High, result.Priority);
        Assert.Null(result.DepartmentId);
        Assert.Equal(3, result.ServiceCategoryId);
        Assert.Equal("createdAt", result.SortBy);
        Assert.Equal("asc", result.SortDirection);
    }
}
