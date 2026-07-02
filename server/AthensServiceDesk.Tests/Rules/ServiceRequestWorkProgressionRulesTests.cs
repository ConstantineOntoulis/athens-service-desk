using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.Rules;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Tests.Rules;

public sealed class ServiceRequestWorkProgressionRulesTests
{
    [Theory]
    [InlineData(ServiceRequestStatus.Assigned, true)]
    [InlineData(ServiceRequestStatus.Scheduled, true)]
    [InlineData(ServiceRequestStatus.Submitted, false)]
    [InlineData(ServiceRequestStatus.UnderReview, false)]
    [InlineData(ServiceRequestStatus.InProgress, false)]
    [InlineData(ServiceRequestStatus.Resolved, false)]
    [InlineData(ServiceRequestStatus.Closed, false)]
    [InlineData(ServiceRequestStatus.Cancelled, false)]
    [InlineData(ServiceRequestStatus.Rejected, false)]
    [InlineData(ServiceRequestStatus.Reopened, false)]
    public void CanStart_ShouldReturnExpectedResult(
        ServiceRequestStatus status,
        bool expected)
    {
        bool result =
            ServiceRequestRules.CanStart(status);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CanWorkOn_ShouldReturnTrue_WhenRequestIsAssignedToCurrentStaff()
    {
        var accessScope =
            new ServiceRequestAccessScope(
                20,
                UserRole.Staff);

        var serviceRequest =
            new ServiceRequest
            {
                AssignedToUserId = 20
            };

        bool result =
            ServiceRequestAccessRules.CanWorkOn(
                accessScope,
                serviceRequest);

        Assert.True(result);
    }

    [Fact]
    public void CanWorkOn_ShouldReturnFalse_WhenRequestIsAssignedToDifferentStaff()
    {
        var accessScope =
            new ServiceRequestAccessScope(
                20,
                UserRole.Staff);

        var serviceRequest =
            new ServiceRequest
            {
                AssignedToUserId = 25
            };

        bool result =
            ServiceRequestAccessRules.CanWorkOn(
                accessScope,
                serviceRequest);

        Assert.False(result);
    }

    [Theory]
    [InlineData(UserRole.Citizen)]
    [InlineData(UserRole.Manager)]
    [InlineData(UserRole.Admin)]
    public void CanWorkOn_ShouldReturnFalse_ForNonStaffRole(
        UserRole role)
    {
        var accessScope =
            new ServiceRequestAccessScope(
                20,
                role);

        var serviceRequest =
            new ServiceRequest
            {
                AssignedToUserId = 20
            };

        bool result =
            ServiceRequestAccessRules.CanWorkOn(
                accessScope,
                serviceRequest);

        Assert.False(result);
    }
}