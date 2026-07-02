using AthensServiceDesk.Application.DTOs.ServiceRequests;

namespace AthensServiceDesk.Application.Interfaces.Services;

public interface IServiceRequestWorkflowService
{
    Task<ServiceRequestDetailsResponse> AssignAsync(
        int serviceRequestId,
        AssignServiceRequestRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceRequestDetailsResponse> StartAsync(
        int serviceRequestId,
        StartServiceRequestRequest request,
        CancellationToken cancelToken = default);

    Task<ServiceRequestDetailsResponse> ResolveAsync(
        int serviceRequestId,
        ResolveServiceRequestRequest request,
        CancellationToken cancelToken = default);
}