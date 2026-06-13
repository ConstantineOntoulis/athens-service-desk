using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Domain.Entities;

namespace AthensServiceDesk.Application.Mappings;

public static class ServiceRequestMapper
{
    public static ServiceRequestResponse ToResponse(
        ServiceRequest serviceRequest)
    {
        return new ServiceRequestResponse
        {
            Id = serviceRequest.Id,
            Title = serviceRequest.Title,
            Status = serviceRequest.Status.ToString(),
            Priority = serviceRequest.Priority.ToString(),
            DepartmentName = serviceRequest.Department?.Name ?? string.Empty,
            ServiceCategoryName = serviceRequest.ServiceCategory?.Name ?? string.Empty,
            CreatedAt = serviceRequest.CreatedAt
        };
    }

    public static ServiceRequestDetailsResponse ToDetailsResponse(
        ServiceRequest serviceRequest)
    {
        return new ServiceRequestDetailsResponse
        {
            Id = serviceRequest.Id,
            Title = serviceRequest.Title,
            Description = serviceRequest.Description,
            Location = serviceRequest.Location,
            Status = serviceRequest.Status.ToString(),
            Priority = serviceRequest.Priority.ToString(),

            DepartmentId = serviceRequest.DepartmentId,
            DepartmentName = serviceRequest.Department?.Name ?? string.Empty,

            ServiceCategoryId = serviceRequest.ServiceCategoryId,
            ServiceCategoryName =
                serviceRequest.ServiceCategory?.Name ?? string.Empty,

            CreatedByUserId = serviceRequest.CreatedByUserId,
            AssignedToUserId = serviceRequest.AssignedToUserId,
            AssignedAt = serviceRequest.AssignedAt,
            ResolvedAt = serviceRequest.ResolvedAt,
            ClosedAt = serviceRequest.ClosedAt,
            CreatedAt = serviceRequest.CreatedAt,
            UpdatedAt = serviceRequest.UpdatedAt
        };
    }
}