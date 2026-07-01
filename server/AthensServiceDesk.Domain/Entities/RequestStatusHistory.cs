using AthensServiceDesk.Domain.Common;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Domain.Entities;

public class RequestStatusHistory : BaseEntity
{
    public int ServiceRequestId { get; set; }

    public ServiceRequest ServiceRequest { get; set; } =
        null!;

    public ServiceRequestStatus? PreviousStatus
    {
        get;
        set;
    }

    public ServiceRequestStatus NewStatus
    {
        get;
        set;
    }

    public int ChangedByUserId { get; set; }

    public AppUser ChangedByUser { get; set; } =
        null!;

    public string? Note { get; set; }
}