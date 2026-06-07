using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Application.DTOs.ServiceRequests;

public class ServiceRequestQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public ServiceRequestStatus? Status { get; set; }

    public ServicePriority? Priority { get; set; }

    public int? DepartmentId { get; set; }

    public int? ServiceCategoryId { get; set; }

    public string? SortBy { get; set; } = "createdAt";

    public string? SortDirection { get; set; } = "desc";
}