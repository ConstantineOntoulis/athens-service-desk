using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AthensServiceDesk.Infrastructure.Repositories;

public class ServiceCategoryRepository : IServiceCategoryRepository
{
    private readonly AppDbContext _dbContext;

    public ServiceCategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceCategories
            .AsNoTracking()
            .AnyAsync(
                category => category.Id == id && category.IsActive,
                cancellationToken);
    }

    public async Task<int?> GetDepartmentIdAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceCategories
            .AsNoTracking()
            .Where(category => category.Id == categoryId && category.IsActive)
            .Select(category => (int?)category.DepartmentId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> BelongsToDepartmentAsync(
        int categoryId,
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceCategories
            .AsNoTracking()
            .AnyAsync(
                category =>
                    category.Id == categoryId
                    && category.DepartmentId == departmentId
                    && category.IsActive
                    && category.Department.IsActive,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceCategory>> GetActiveByDepartmentIdAsync(
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceCategories
            .AsNoTracking()
            .Where(
                category =>
                    category.DepartmentId == departmentId
                    && category.IsActive
                    && category.Department.IsActive)
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }
}