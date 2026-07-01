using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Infrastructure.Persistence;

namespace AthensServiceDesk.Infrastructure.Repositories;

public sealed class RequestStatusHistoryRepository
    : IRequestStatusHistoryRepository
{
    private readonly AppDbContext _dbContext;

    public RequestStatusHistoryRepository(
        AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        RequestStatusHistory statusHistory,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RequestStatusHistory.AddAsync(
            statusHistory,
            cancellationToken);
    }
}