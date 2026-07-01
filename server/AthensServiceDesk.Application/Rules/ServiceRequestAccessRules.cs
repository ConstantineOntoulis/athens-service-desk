using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Application.Rules;

public static class ServiceRequestAccessRules
{
    public static bool CanCreate(UserRole role)
    {
        return role == UserRole.Citizen;
    }

    public static bool CanView(
        ServiceRequestAccessScope accessScope,
        ServiceRequest serviceRequest)
    {
        return accessScope.Role switch
        {
            UserRole.Citizen =>
                serviceRequest.CreatedByUserId ==
                accessScope.UserId,

            UserRole.Staff =>
                serviceRequest.AssignedToUserId ==
                accessScope.UserId,

            UserRole.Manager => true,

            UserRole.Admin => true,

            _ => false
        };
    }

    public static bool CanEditDetails(
        ServiceRequestAccessScope accessScope,
        ServiceRequest serviceRequest)
    {
        return accessScope.Role == UserRole.Citizen
            && serviceRequest.CreatedByUserId ==
                accessScope.UserId;
    }
}