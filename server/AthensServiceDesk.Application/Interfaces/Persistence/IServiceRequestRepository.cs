using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Domain.Entities;

namespace AthensServiceDesk.Application.Interfaces.Persistence;

public interface IServiceRequestRepository
{
    Task<ServiceRequest?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ServiceRequest?> GetByIdForUpdateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceRequest>> ListAsync(
        ServiceRequestQuery query,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        ServiceRequestQuery query,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ServiceRequest serviceRequest,
        CancellationToken cancellationToken = default);
    
}