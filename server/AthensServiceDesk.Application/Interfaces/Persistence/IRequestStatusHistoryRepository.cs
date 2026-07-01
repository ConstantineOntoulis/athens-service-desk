using AthensServiceDesk.Domain.Entities;

namespace AthensServiceDesk.Application.Interfaces.Persistence;

public interface IRequestStatusHistoryRepository
{
    Task AddAsync(
        RequestStatusHistory statusHistory,
        CancellationToken cancellationToken = default);
}