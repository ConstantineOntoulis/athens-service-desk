using AthensServiceDesk.Domain.Entities;

namespace AthensServiceDesk.Application.Interfaces.Persistence;

public interface IServiceCategoryRepository
{
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task<int?> GetDepartmentIdAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceCategory>> GetActiveByDepartmentIdAsync(
        int departmentId,
        CancellationToken cancellationToken = default);
}