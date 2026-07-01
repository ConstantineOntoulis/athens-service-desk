using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Application.Rules;

public static class ServiceRequestRules
{
    public static bool CanEdit(
        ServiceRequestStatus status)
    {
        return status is
            ServiceRequestStatus.Submitted
            or ServiceRequestStatus.UnderReview
            or ServiceRequestStatus.Reopened;
    }

    public static bool CanCancel(
        ServiceRequestStatus status)
    {
        return CanTransitionStatus(
            status,
            ServiceRequestStatus.Cancelled);
    }

    public static bool CanAssign(
        ServiceRequestStatus status)
    {
        return CanTransitionStatus(
            status,
            ServiceRequestStatus.Assigned);
    }

    public static bool CanResolve(
        ServiceRequestStatus status)
    {
        return CanTransitionStatus(
            status,
            ServiceRequestStatus.Resolved);
    }

    public static bool CanClose(
        ServiceRequestStatus status)
    {
        return CanTransitionStatus(
            status,
            ServiceRequestStatus.Closed);
    }

    public static bool IsCategoryValidForDepartment(
        int categoryDepartmentId,
        int selectedDepartmentId)
    {
        return categoryDepartmentId > 0
            && selectedDepartmentId > 0
            && categoryDepartmentId ==
                selectedDepartmentId;
    }

    public static bool CanTransitionStatus(
        ServiceRequestStatus currentStatus,
        ServiceRequestStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return false;
        }

        return currentStatus switch
        {
            ServiceRequestStatus.Submitted =>
                nextStatus is
                    ServiceRequestStatus.UnderReview
                    or ServiceRequestStatus.Assigned
                    or ServiceRequestStatus.Cancelled
                    or ServiceRequestStatus.Rejected,

            ServiceRequestStatus.UnderReview =>
                nextStatus is
                    ServiceRequestStatus.Assigned
                    or ServiceRequestStatus.Cancelled
                    or ServiceRequestStatus.Rejected,

            ServiceRequestStatus.Assigned =>
                nextStatus is
                    ServiceRequestStatus.Scheduled
                    or ServiceRequestStatus.InProgress
                    or ServiceRequestStatus.Cancelled,

            ServiceRequestStatus.Scheduled =>
                nextStatus is
                    ServiceRequestStatus.InProgress
                    or ServiceRequestStatus.Cancelled,

            ServiceRequestStatus.InProgress =>
                nextStatus is
                    ServiceRequestStatus.Resolved
                    or ServiceRequestStatus.Cancelled,

            ServiceRequestStatus.Resolved =>
                nextStatus is
                    ServiceRequestStatus.Closed
                    or ServiceRequestStatus.Reopened,

            ServiceRequestStatus.Closed =>
                nextStatus ==
                    ServiceRequestStatus.Reopened,

            ServiceRequestStatus.Reopened =>
                nextStatus is
                    ServiceRequestStatus.UnderReview
                    or ServiceRequestStatus.Assigned,

            ServiceRequestStatus.Rejected => false,

            ServiceRequestStatus.Cancelled => false,

            _ => false
        };
    }
}