using AthensServiceDesk.Domain.Common;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Domain.Entities;

public class ServiceRequest : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Submitted;

    public ServicePriority Priority { get; set; } = ServicePriority.Medium;

    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    public int ServiceCategoryId { get; set; }

    public ServiceCategory ServiceCategory { get; set; } = null!;

    public int? CreatedByUserId { get; set; }

    public int? AssignedToUserId { get; set; }

    public DateTimeOffset? AssignedAt { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }
}
