using AthensServiceDesk.Domain.Entities;

namespace AthensServiceDesk.Application.Interfaces.Persistence;

public interface IDepartmentRepository
{
    Task<bool> ExistsAsync(
        int id, 
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Department>> GetActiveAsync(CancellationToken cancellationToken = default);
}