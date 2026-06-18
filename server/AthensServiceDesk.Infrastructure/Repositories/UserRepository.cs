using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AthensServiceDesk.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AppUser?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user =>
                    user.NormalizedEmail == normalizedEmail
                    && user.IsActive,
                cancellationToken);
    }

    public Task<AppUser?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.Id == id && user.IsActive,
                cancellationToken);
    }
}