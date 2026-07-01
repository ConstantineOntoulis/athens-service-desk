using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.Rules;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Tests.Rules;

public sealed class ServiceRequestAccessRulesTests
{
    [Fact]
    public void CanCreate_ShouldReturnTrue_ForCitizen()
    {
        bool result =
            ServiceRequestAccessRules.CanCreate(
                UserRole.Citizen);

        Assert.True(result);
    }

    [Theory]
    [InlineData(UserRole.Staff)]
    [InlineData(UserRole.Manager)]
    [InlineData(UserRole.Admin)]
    public void CanCreate_ShouldReturnFalse_ForNonCitizen(
        UserRole role)
    {
        bool result =
            ServiceRequestAccessRules.CanCreate(role);

        Assert.False(result);
    }

    [Fact]
    public void CanView_ShouldReturnTrue_WhenCitizenOwnsRequest()
    {
        var accessScope =
            new ServiceRequestAccessScope(
                5,
                UserRole.Citizen);

        var serviceRequest =
            new ServiceRequest
            {
                CreatedByUserId = 5
            };

        bool result =
            ServiceRequestAccessRules.CanView(
                accessScope,
                serviceRequest);

        Assert.True(result);
    }

    [Fact]
    public void CanView_ShouldReturnFalse_WhenCitizenDoesNotOwnRequest()
    {
        var accessScope =
            new ServiceRequestAccessScope(
                5,
                UserRole.Citizen);

        var serviceRequest =
            new ServiceRequest
            {
                CreatedByUserId = 8
            };

        bool result =
            ServiceRequestAccessRules.CanView(
                accessScope,
                serviceRequest);

        Assert.False(result);
    }

    [Fact]
    public void CanView_ShouldReturnTrue_WhenRequestIsAssignedToStaff()
    {
        var accessScope =
            new ServiceRequestAccessScope(
                12,
                UserRole.Staff);

        var serviceRequest =
            new ServiceRequest
            {
                AssignedToUserId = 12
            };

        bool result =
            ServiceRequestAccessRules.CanView(
                accessScope,
                serviceRequest);

        Assert.True(result);
    }

    [Fact]
    public void CanView_ShouldReturnFalse_WhenRequestIsAssignedToDifferentStaff()
    {
        var accessScope =
            new ServiceRequestAccessScope(
                12,
                UserRole.Staff);

        var serviceRequest =
            new ServiceRequest
            {
                AssignedToUserId = 20
            };

        bool result =
            ServiceRequestAccessRules.CanView(
                accessScope,
                serviceRequest);

        Assert.False(result);
    }

    [Theory]
    [InlineData(UserRole.Manager)]
    [InlineData(UserRole.Admin)]
    public void CanView_ShouldReturnTrue_ForPrivilegedRoles(
        UserRole role)
    {
        var accessScope =
            new ServiceRequestAccessScope(
                30,
                role);

        var serviceRequest =
            new ServiceRequest
            {
                CreatedByUserId = 1,
                AssignedToUserId = 2
            };

        bool result =
            ServiceRequestAccessRules.CanView(
                accessScope,
                serviceRequest);

        Assert.True(result);
    }

    [Fact]
    public void CanEditDetails_ShouldRequireCitizenOwnership()
    {
        var ownerScope =
            new ServiceRequestAccessScope(
                5,
                UserRole.Citizen);

        var otherCitizenScope =
            new ServiceRequestAccessScope(
                8,
                UserRole.Citizen);

        var staffScope =
            new ServiceRequestAccessScope(
                5,
                UserRole.Staff);

        var serviceRequest =
            new ServiceRequest
            {
                CreatedByUserId = 5
            };

        Assert.True(
            ServiceRequestAccessRules.CanEditDetails(
                ownerScope,
                serviceRequest));

        Assert.False(
            ServiceRequestAccessRules.CanEditDetails(
                otherCitizenScope,
                serviceRequest));

        Assert.False(
            ServiceRequestAccessRules.CanEditDetails(
                staffScope,
                serviceRequest));
    }

    [Theory]
    [InlineData(UserRole.Manager, true)]
    [InlineData(UserRole.Admin, true)]
    [InlineData(UserRole.Citizen, false)]
    [InlineData(UserRole.Staff, false)]
    public void CanManageAssignments_ShouldReturnExpectedResult(
        UserRole role,
        bool expected)
    {
        bool result =
            ServiceRequestAccessRules
                .CanManageAssignments(role);

        Assert.Equal(expected, result);
    }
}