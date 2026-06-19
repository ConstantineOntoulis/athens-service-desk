using AthensServiceDesk.Domain.Entities;

namespace AthensServiceDesk.Application.Interfaces.Persistence;

public interface IUserRepository
{
    Task<AppUser?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default);

    Task<AppUser?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken= default);
}
