using AthensServiceDesk.Application.Rules;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Tests.Rules;

public class ServiceRequestRulesTests
{
    [Theory]
    [InlineData(ServiceRequestStatus.Submitted, true)]
    [InlineData(ServiceRequestStatus.UnderReview, true)]
    [InlineData(ServiceRequestStatus.Reopened, true)]
    [InlineData(ServiceRequestStatus.Assigned, false)]
    [InlineData(ServiceRequestStatus.Scheduled, false)]
    [InlineData(ServiceRequestStatus.InProgress, false)]
    [InlineData(ServiceRequestStatus.Resolved, false)]
    [InlineData(ServiceRequestStatus.Closed, false)]
    [InlineData(ServiceRequestStatus.Cancelled, false)]
    [InlineData(ServiceRequestStatus.Rejected, false)]
    public void CanEdit_ShouldReturnExpectedResult(
        ServiceRequestStatus status,
        bool expected)
    {
        bool result = ServiceRequestRules.CanEdit(status);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.Submitted, true)]
    [InlineData(ServiceRequestStatus.UnderReview, true)]
    [InlineData(ServiceRequestStatus.Assigned, true)]
    [InlineData(ServiceRequestStatus.Scheduled, true)]
    [InlineData(ServiceRequestStatus.InProgress, false)]
    [InlineData(ServiceRequestStatus.Resolved, false)]
    [InlineData(ServiceRequestStatus.Closed, false)]
    [InlineData(ServiceRequestStatus.Cancelled, false)]
    [InlineData(ServiceRequestStatus.Rejected, false)]
    [InlineData(ServiceRequestStatus.Reopened, false)]
    public void CanCancel_ShouldReturnExpectedResult(ServiceRequestStatus status, bool expected)
    {
        bool result = ServiceRequestRules.CanCancel(status);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.Submitted, true)]
    [InlineData(ServiceRequestStatus.UnderReview, true)]
    [InlineData(ServiceRequestStatus.Reopened, true)]
    [InlineData(ServiceRequestStatus.Assigned, false)]
    [InlineData(ServiceRequestStatus.Scheduled, false)]
    [InlineData(ServiceRequestStatus.InProgress, false)]
    [InlineData(ServiceRequestStatus.Resolved, false)]
    [InlineData(ServiceRequestStatus.Closed, false)]
    [InlineData(ServiceRequestStatus.Cancelled, false)]
    [InlineData(ServiceRequestStatus.Rejected, false)]
    public void CanAssign_ShouldReturnExpectedResult(ServiceRequestStatus status, bool expected)
    {
        bool result = ServiceRequestRules.CanAssign(status);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.InProgress, true)]
    [InlineData(ServiceRequestStatus.Submitted, false)]
    [InlineData(ServiceRequestStatus.Assigned, false)]
    [InlineData(ServiceRequestStatus.Resolved, false)]
    [InlineData(ServiceRequestStatus.Closed, false)]
    public void CanResolve_ShouldReturnTrueOnlyForInProgress(ServiceRequestStatus status, bool expected)
    {
        bool result = ServiceRequestRules.CanResolve(status);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.Resolved, true)]
    [InlineData(ServiceRequestStatus.Submitted, false)]
    [InlineData(ServiceRequestStatus.InProgress, false)]
    [InlineData(ServiceRequestStatus.Closed, false)]
    [InlineData(ServiceRequestStatus.Cancelled, false)]
    public void CanClose_ShouldReturnTrueOnlyForResolved(ServiceRequestStatus status, bool expected)
    {
        bool result = ServiceRequestRules.CanClose(status);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(2, 2, true)]
    [InlineData(1, 2, false)]
    [InlineData(0, 1, false)]
    [InlineData(1, 0, false)]
    [InlineData(-1, 1, false)]
    [InlineData(1, -1, false)]
    public void IsCategoryValidForDepartment_ShouldReturnExpectedResult(
        int categoryDepartmentId,
        int selectedDepartmentId,
        bool expected)
    {
        bool result = ServiceRequestRules.IsCategoryValidForDepartment(
            categoryDepartmentId,
            selectedDepartmentId);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.Submitted, ServiceRequestStatus.UnderReview, true)]
    [InlineData(ServiceRequestStatus.Submitted, ServiceRequestStatus.Cancelled, true)]
    [InlineData(ServiceRequestStatus.Submitted, ServiceRequestStatus.Rejected, true)]
    [InlineData(ServiceRequestStatus.Submitted, ServiceRequestStatus.InProgress, false)]

    [InlineData(ServiceRequestStatus.UnderReview, ServiceRequestStatus.Assigned, true)]
    [InlineData(ServiceRequestStatus.UnderReview, ServiceRequestStatus.Cancelled, true)]
    [InlineData(ServiceRequestStatus.UnderReview, ServiceRequestStatus.Rejected, true)]
    [InlineData(ServiceRequestStatus.UnderReview, ServiceRequestStatus.Closed, false)]

    [InlineData(ServiceRequestStatus.Assigned, ServiceRequestStatus.Scheduled, true)]
    [InlineData(ServiceRequestStatus.Assigned, ServiceRequestStatus.InProgress, true)]
    [InlineData(ServiceRequestStatus.Assigned, ServiceRequestStatus.Cancelled, true)]
    [InlineData(ServiceRequestStatus.Assigned, ServiceRequestStatus.Resolved, false)]

    [InlineData(ServiceRequestStatus.Scheduled, ServiceRequestStatus.InProgress, true)]
    [InlineData(ServiceRequestStatus.Scheduled, ServiceRequestStatus.Cancelled, true)]
    [InlineData(ServiceRequestStatus.Scheduled, ServiceRequestStatus.Resolved, false)]

    [InlineData(ServiceRequestStatus.InProgress, ServiceRequestStatus.Resolved, true)]
    [InlineData(ServiceRequestStatus.InProgress, ServiceRequestStatus.Cancelled, true)]
    [InlineData(ServiceRequestStatus.InProgress, ServiceRequestStatus.Closed, false)]

    [InlineData(ServiceRequestStatus.Resolved, ServiceRequestStatus.Closed, true)]
    [InlineData(ServiceRequestStatus.Resolved, ServiceRequestStatus.Reopened, true)]
    [InlineData(ServiceRequestStatus.Resolved, ServiceRequestStatus.Assigned, false)]

    [InlineData(ServiceRequestStatus.Closed, ServiceRequestStatus.Reopened, true)]
    [InlineData(ServiceRequestStatus.Closed, ServiceRequestStatus.InProgress, false)]

    [InlineData(ServiceRequestStatus.Reopened, ServiceRequestStatus.UnderReview, true)]
    [InlineData(ServiceRequestStatus.Reopened, ServiceRequestStatus.Closed, false)]

    [InlineData(ServiceRequestStatus.Rejected, ServiceRequestStatus.UnderReview, false)]
    [InlineData(ServiceRequestStatus.Cancelled, ServiceRequestStatus.UnderReview, false)]
    public void CanTransitionStatus_ShouldReturnExpectedResult(
        ServiceRequestStatus currentStatus,
        ServiceRequestStatus nextStatus,
        bool expected)
    {
        bool result = ServiceRequestRules.CanTransitionStatus(currentStatus, nextStatus);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.Submitted)]
    [InlineData(ServiceRequestStatus.UnderReview)]
    [InlineData(ServiceRequestStatus.Assigned)]
    [InlineData(ServiceRequestStatus.Scheduled)]
    [InlineData(ServiceRequestStatus.InProgress)]
    [InlineData(ServiceRequestStatus.Resolved)]
    [InlineData(ServiceRequestStatus.Closed)]
    [InlineData(ServiceRequestStatus.Cancelled)]
    [InlineData(ServiceRequestStatus.Rejected)]
    [InlineData(ServiceRequestStatus.Reopened)]
    public void CanTransitionStatus_ShouldReturnFalse_WhenStatusDoesNotChange(
        ServiceRequestStatus status)
    {
        bool result = ServiceRequestRules.CanTransitionStatus(status, status);

        Assert.False(result);
    }
}