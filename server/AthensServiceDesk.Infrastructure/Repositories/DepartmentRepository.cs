using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AthensServiceDesk.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _dbContext;

    public DepartmentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .AnyAsync(
                department => department.Id == id && department.IsActive,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Department>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .Where(department => department.IsActive)
            .OrderBy(department => department.Name)
            .ToListAsync(cancellationToken);
    }
}