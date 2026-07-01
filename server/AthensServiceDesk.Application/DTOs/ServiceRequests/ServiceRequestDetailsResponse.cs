namespace AthensServiceDesk.Application.DTOs.ServiceRequests;

public class ServiceRequestDetailsResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public int ServiceCategoryId { get; set; }

    public string ServiceCategoryName { get; set; } = string.Empty;

    public int? CreatedByUserId { get; set; }

    public int? AssignedToUserId { get; set; }

    public DateTimeOffset? AssignedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public IReadOnlyList<RequestStatusHistoryResponse>
        StatusHistory
    { get; set; } = [];
}