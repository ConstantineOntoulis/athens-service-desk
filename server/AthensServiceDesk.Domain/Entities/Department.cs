using AthensServiceDesk.Domain.Common;

namespace AthensServiceDesk.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ServiceCategory> Categories { get; set; } = new List<ServiceCategory>();

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
