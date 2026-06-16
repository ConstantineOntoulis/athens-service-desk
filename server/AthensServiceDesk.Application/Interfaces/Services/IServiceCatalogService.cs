using AthensServiceDesk.Application.DTOs.Lookups;

namespace AthensServiceDesk.Application.Interfaces.Services;

public interface IServiceCatalogService
{
    Task<IReadOnlyList<DepartmentResponse>> GetDepartmentsAsync(
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceCategoryResponse>>
        GetServiceCategoriesByDepartmentIdAsync(
            int departmentId,
            CancellationToken cancellationToken = default);
}
