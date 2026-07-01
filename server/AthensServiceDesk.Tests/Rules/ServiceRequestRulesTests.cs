using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.Interfaces.Security;
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
        var scope =
            new ServiceRequestAccessScope(
                5,
                UserRole.Citizen);

        var request =
            new ServiceRequest
            {
                CreatedByUserId = 5
            };

        Assert.True(
            ServiceRequestAccessRules.CanView(
                scope,
                request));
    }

    [Fact]
    public void CanView_ShouldReturnFalse_WhenCitizenDoesNotOwnRequest()
    {
        var scope =
            new ServiceRequestAccessScope(
                5,
                UserRole.Citizen);

        var request =
            new ServiceRequest
            {
                CreatedByUserId = 8
            };

        Assert.False(
            ServiceRequestAccessRules.CanView(
                scope,
                request));
    }

    [Fact]
    public void CanView_ShouldReturnTrue_WhenRequestIsAssignedToStaff()
    {
        var scope =
            new ServiceRequestAccessScope(
                12,
                UserRole.Staff);

        var request =
            new ServiceRequest
            {
                AssignedToUserId = 12
            };

        Assert.True(
            ServiceRequestAccessRules.CanView(
                scope,
                request));
    }

    [Fact]
    public void CanView_ShouldReturnFalse_WhenRequestIsAssignedToDifferentStaff()
    {
        var scope =
            new ServiceRequestAccessScope(
                12,
                UserRole.Staff);

        var request =
            new ServiceRequest
            {
                AssignedToUserId = 20
            };

        Assert.False(
            ServiceRequestAccessRules.CanView(
                scope,
                request));
    }

    [Theory]
    [InlineData(UserRole.Manager)]
    [InlineData(UserRole.Admin)]
    public void CanView_ShouldReturnTrue_ForPrivilegedRoles(
        UserRole role)
    {
        var scope =
            new ServiceRequestAccessScope(
                30,
                role);

        var request =
            new ServiceRequest
            {
                CreatedByUserId = 1,
                AssignedToUserId = 2
            };

        Assert.True(
            ServiceRequestAccessRules.CanView(
                scope,
                request));
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

        var request =
            new ServiceRequest
            {
                CreatedByUserId = 5
            };

        Assert.True(
            ServiceRequestAccessRules.CanEditDetails(
                ownerScope,
                request));

        Assert.False(
            ServiceRequestAccessRules.CanEditDetails(
                otherCitizenScope,
                request));

        Assert.False(
            ServiceRequestAccessRules.CanEditDetails(
                staffScope,
                request));
    }
}