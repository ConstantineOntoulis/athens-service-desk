using AthensServiceDesk.Application.DTOs.ServiceRequests;

namespace AthensServiceDesk.Application.Interfaces.Services;

public interface IServiceRequestWorkflowService
{
    Task<ServiceRequestDetailsResponse> AssignAsync(
        int serviceRequestId,
        AssignServiceRequestRequest request,
        CancellationToken cancellationToken = default);
}