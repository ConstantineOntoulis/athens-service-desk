using AthensServiceDesk.Application.DTOs.Common;
using AthensServiceDesk.Application.DTOs.ServiceRequests;

namespace AthensServiceDesk.Application.Interfaces.Services;

public interface IServiceRequestService
{
    Task<PagedResponse<ServiceRequestResponse>> GetPagedAsync(
        ServiceRequestQuery query,
        CancellationToken cancellationToken = default);

    Task<ServiceRequestDetailsResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ServiceRequestDetailsResponse> CreateAsync(
        CreateServiceRequestRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceRequestDetailsResponse> UpdateAsync(
        int id,
        UpdateServiceRequestRequest request,
        CancellationToken cancellationToken = default);
}