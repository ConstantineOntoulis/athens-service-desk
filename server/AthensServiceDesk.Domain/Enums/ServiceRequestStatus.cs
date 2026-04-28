namespace AthensServiceDesk.Domain.Enums;

public enum ServiceRequestStatus
{
    Submitted = 1,
    UnderReview = 2,
    Assigned = 3,
    Scheduled = 4,
    InProgress = 5,
    Resolved = 6,
    Closed = 7,
    Rejected = 8,
    Cancelled = 9,
    Reopened = 10
}
