using AthensServiceDesk.Application.Rules;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Tests.Rules;

public sealed class ServiceRequestRulesTests
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
        bool result =
            ServiceRequestRules.CanEdit(status);

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
    public void CanAssign_ShouldReturnExpectedResult(
        ServiceRequestStatus status,
        bool expected)
    {
        bool result =
            ServiceRequestRules.CanAssign(status);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.Submitted, true)]
    [InlineData(ServiceRequestStatus.UnderReview, true)]
    [InlineData(ServiceRequestStatus.Assigned, true)]
    [InlineData(ServiceRequestStatus.Scheduled, true)]
    [InlineData(ServiceRequestStatus.InProgress, true)]
    [InlineData(ServiceRequestStatus.Resolved, false)]
    [InlineData(ServiceRequestStatus.Closed, false)]
    [InlineData(ServiceRequestStatus.Cancelled, false)]
    [InlineData(ServiceRequestStatus.Rejected, false)]
    [InlineData(ServiceRequestStatus.Reopened, false)]
    public void CanCancel_ShouldReturnExpectedResult(
        ServiceRequestStatus status,
        bool expected)
    {
        bool result =
            ServiceRequestRules.CanCancel(status);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.InProgress, true)]
    [InlineData(ServiceRequestStatus.Submitted, false)]
    [InlineData(ServiceRequestStatus.Assigned, false)]
    [InlineData(ServiceRequestStatus.Resolved, false)]
    [InlineData(ServiceRequestStatus.Closed, false)]
    public void CanResolve_ShouldReturnExpectedResult(
        ServiceRequestStatus status,
        bool expected)
    {
        bool result =
            ServiceRequestRules.CanResolve(status);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.Resolved, true)]
    [InlineData(ServiceRequestStatus.Submitted, false)]
    [InlineData(ServiceRequestStatus.InProgress, false)]
    [InlineData(ServiceRequestStatus.Closed, false)]
    public void CanClose_ShouldReturnExpectedResult(
        ServiceRequestStatus status,
        bool expected)
    {
        bool result =
            ServiceRequestRules.CanClose(status);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(
        ServiceRequestStatus.Submitted,
        ServiceRequestStatus.UnderReview)]
    [InlineData(
        ServiceRequestStatus.Submitted,
        ServiceRequestStatus.Assigned)]
    [InlineData(
        ServiceRequestStatus.UnderReview,
        ServiceRequestStatus.Assigned)]
    [InlineData(
        ServiceRequestStatus.Assigned,
        ServiceRequestStatus.InProgress)]
    [InlineData(
        ServiceRequestStatus.Scheduled,
        ServiceRequestStatus.InProgress)]
    [InlineData(
        ServiceRequestStatus.InProgress,
        ServiceRequestStatus.Resolved)]
    [InlineData(
        ServiceRequestStatus.Resolved,
        ServiceRequestStatus.Closed)]
    [InlineData(
        ServiceRequestStatus.Resolved,
        ServiceRequestStatus.Reopened)]
    [InlineData(
        ServiceRequestStatus.Closed,
        ServiceRequestStatus.Reopened)]
    [InlineData(
        ServiceRequestStatus.Reopened,
        ServiceRequestStatus.Assigned)]
    public void CanTransitionStatus_ShouldReturnTrue_ForAllowedTransition(
        ServiceRequestStatus currentStatus,
        ServiceRequestStatus nextStatus)
    {
        bool result =
            ServiceRequestRules.CanTransitionStatus(
                currentStatus,
                nextStatus);

        Assert.True(result);
    }

    [Theory]
    [InlineData(
        ServiceRequestStatus.Submitted,
        ServiceRequestStatus.Resolved)]
    [InlineData(
        ServiceRequestStatus.Assigned,
        ServiceRequestStatus.Closed)]
    [InlineData(
        ServiceRequestStatus.Rejected,
        ServiceRequestStatus.UnderReview)]
    [InlineData(
        ServiceRequestStatus.Cancelled,
        ServiceRequestStatus.Reopened)]
    [InlineData(
        ServiceRequestStatus.InProgress,
        ServiceRequestStatus.Submitted)]
    public void CanTransitionStatus_ShouldReturnFalse_ForInvalidTransition(
        ServiceRequestStatus currentStatus,
        ServiceRequestStatus nextStatus)
    {
        bool result =
            ServiceRequestRules.CanTransitionStatus(
                currentStatus,
                nextStatus);

        Assert.False(result);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.Submitted)]
    [InlineData(ServiceRequestStatus.InProgress)]
    [InlineData(ServiceRequestStatus.Closed)]
    public void CanTransitionStatus_ShouldReturnFalse_WhenStatusesAreTheSame(
        ServiceRequestStatus status)
    {
        bool result =
            ServiceRequestRules.CanTransitionStatus(
                status,
                status);

        Assert.False(result);
    }

    [Theory]
    [InlineData(2, 2, true)]
    [InlineData(2, 3, false)]
    [InlineData(0, 2, false)]
    [InlineData(2, 0, false)]
    public void IsCategoryValidForDepartment_ShouldReturnExpectedResult(
        int categoryDepartmentId,
        int selectedDepartmentId,
        bool expected)
    {
        bool result =
            ServiceRequestRules
                .IsCategoryValidForDepartment(
                    categoryDepartmentId,
                    selectedDepartmentId);

        Assert.Equal(expected, result);
    }
}