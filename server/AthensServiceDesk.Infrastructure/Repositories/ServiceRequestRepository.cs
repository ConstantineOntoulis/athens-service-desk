using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;
using AthensServiceDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AthensServiceDesk.Infrastructure.Repositories;

public sealed class ServiceRequestRepository
    : IServiceRequestRepository
{
    private readonly AppDbContext _dbContext;

    public ServiceRequestRepository(
        AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ServiceRequest?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceRequests
            .AsNoTracking()
            .Include(request => request.Department)
            .Include(request => request.ServiceCategory)
            .FirstOrDefaultAsync(
                request => request.Id == id,
                cancellationToken);
    }

    public Task<ServiceRequest?> GetByIdForUpdateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceRequests
            .FirstOrDefaultAsync(
                request => request.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceRequest>>
        ListAsync(
            ServiceRequestQuery query,
            ServiceRequestAccessScope accessScope,
            CancellationToken cancellationToken = default)
    {
        IQueryable<ServiceRequest> requests =
            _dbContext.ServiceRequests
                .AsNoTracking()
                .Include(request => request.Department)
                .Include(request => request.ServiceCategory);

        requests =
            ApplyAccessScope(
                requests,
                accessScope);

        requests =
            ApplyFilters(
                requests,
                query);

        requests =
            ApplySorting(
                requests,
                query);

        return await requests
            .Skip(
                (query.Page - 1)
                * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        ServiceRequestQuery query,
        ServiceRequestAccessScope accessScope,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ServiceRequest> requests =
            _dbContext.ServiceRequests
                .AsNoTracking();

        requests =
            ApplyAccessScope(
                requests,
                accessScope);

        requests =
            ApplyFilters(
                requests,
                query);

        return await requests.CountAsync(
            cancellationToken);
    }

    public async Task AddAsync(
        ServiceRequest serviceRequest,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ServiceRequests.AddAsync(
            serviceRequest,
            cancellationToken);
    }

    private static IQueryable<ServiceRequest>
        ApplyAccessScope(
            IQueryable<ServiceRequest> requests,
            ServiceRequestAccessScope accessScope)
    {
        return accessScope.Role switch
        {
            UserRole.Citizen =>
                requests.Where(
                    request =>
                        request.CreatedByUserId ==
                        accessScope.UserId),

            UserRole.Staff =>
                requests.Where(
                    request =>
                        request.AssignedToUserId ==
                        accessScope.UserId),

            UserRole.Manager => requests,

            UserRole.Admin => requests,

            _ =>
                requests.Where(
                    _ => false)
        };
    }

    private static IQueryable<ServiceRequest>
        ApplyFilters(
            IQueryable<ServiceRequest> requests,
            ServiceRequestQuery query)
    {
        if (!string.IsNullOrWhiteSpace(
                query.Search))
        {
            string search =
                query.Search.Trim();

            requests =
                requests.Where(
                    request =>
                        request.Title.Contains(search)
                        || request.Description.Contains(search)
                        || request.Location.Contains(search)
                        || request.Department.Name.Contains(search)
                        || request.ServiceCategory.Name.Contains(search));
        }

        if (query.Status.HasValue)
        {
            requests =
                requests.Where(
                    request =>
                        request.Status ==
                        query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            requests =
                requests.Where(
                    request =>
                        request.Priority ==
                        query.Priority.Value);
        }

        if (query.DepartmentId.HasValue)
        {
            requests =
                requests.Where(
                    request =>
                        request.DepartmentId ==
                        query.DepartmentId.Value);
        }

        if (query.ServiceCategoryId.HasValue)
        {
            requests =
                requests.Where(
                    request =>
                        request.ServiceCategoryId ==
                        query.ServiceCategoryId.Value);
        }

        return requests;
    }

    private static IQueryable<ServiceRequest>
        ApplySorting(
            IQueryable<ServiceRequest> requests,
            ServiceRequestQuery query)
    {
        bool ascending =
            string.Equals(
                query.SortDirection,
                "asc",
                StringComparison.OrdinalIgnoreCase);

        return query.SortBy?
            .ToLowerInvariant() switch
        {
            "title" =>
                ascending
                    ? requests.OrderBy(
                        request => request.Title)
                    : requests.OrderByDescending(
                        request => request.Title),

            "status" =>
                ascending
                    ? requests.OrderBy(
                        request => request.Status)
                    : requests.OrderByDescending(
                        request => request.Status),

            "priority" =>
                ascending
                    ? requests.OrderBy(
                        request => request.Priority)
                    : requests.OrderByDescending(
                        request => request.Priority),

            _ =>
                ascending
                    ? requests.OrderBy(
                        request => request.CreatedAt)
                    : requests.OrderByDescending(
                        request => request.CreatedAt)
        };
    }
}