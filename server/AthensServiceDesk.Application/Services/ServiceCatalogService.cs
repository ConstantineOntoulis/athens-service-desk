using AthensServiceDesk.Application.Common.Exceptions;
using AthensServiceDesk.Application.DTOs.Lookups;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Application.Interfaces.Services;
using AthensServiceDesk.Domain.Entities;

namespace AthensServiceDesk.Application.Services;

public sealed class ServiceCatalogService : IServiceCatalogService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IServiceCategoryRepository _serviceCategoryRepository;

    public ServiceCatalogService(
        IDepartmentRepository departmentRepository,
        IServiceCategoryRepository serviceCategoryRepository)
    {
        _departmentRepository = departmentRepository;
        _serviceCategoryRepository = serviceCategoryRepository;
    }

    public async Task<IReadOnlyList<DepartmentResponse>>
        GetDepartmentsAsync(
            CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Department> departments =
            await _departmentRepository.GetActiveAsync(
                cancellationToken);

        return departments
            .Select(department => new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ServiceCategoryResponse>>
        GetServiceCategoriesByDepartmentIdAsync(
            int departmentId,
            CancellationToken cancellationToken = default)
    {
        if (departmentId < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(departmentId),
                "The department identifier must be greater than zero.");
        }

        bool departmentExists =
            await _departmentRepository.ExistsAsync(
                departmentId,
                cancellationToken);

        if (!departmentExists)
        {
            throw new NotFoundException(
                "Department",
                departmentId);
        }

        IReadOnlyList<ServiceCategory> categories =
            await _serviceCategoryRepository
                .GetActiveByDepartmentIdAsync(
                    departmentId,
                    cancellationToken);

        return categories
            .Select(category => new ServiceCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DepartmentId = category.DepartmentId
            })
            .ToList();
    }
}