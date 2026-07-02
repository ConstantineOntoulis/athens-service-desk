using AthensServiceDesk.Application.Common.Exceptions;
using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Application.Interfaces.Services;
using AthensServiceDesk.Application.Mappings;
using AthensServiceDesk.Application.Rules;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Application.Services;

public sealed class ServiceRequestWorkflowService
    : IServiceRequestWorkflowService
{
    private readonly IServiceRequestRepository
        _serviceRequestRepository;

    private readonly IRequestStatusHistoryRepository
        _statusHistoryRepository;

    private readonly IUserRepository
        _userRepository;

    private readonly IUnitOfWork
        _unitOfWork;

    private readonly ICurrentUserService
        _currentUserService;

    private readonly TimeProvider
        _timeProvider;

    public ServiceRequestWorkflowService(
        IServiceRequestRepository serviceRequestRepository,
        IRequestStatusHistoryRepository statusHistoryRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        TimeProvider timeProvider)
    {
        _serviceRequestRepository =
            serviceRequestRepository;

        _statusHistoryRepository =
            statusHistoryRepository;

        _userRepository =
            userRepository;

        _unitOfWork =
            unitOfWork;

        _currentUserService =
            currentUserService;

        _timeProvider =
            timeProvider;
    }

    public async Task<ServiceRequestDetailsResponse>
        AssignAsync(
            int serviceRequestId,
            AssignServiceRequestRequest request,
            CancellationToken cancellationToken = default)
    {
        EnsureValidId(serviceRequestId);
        ArgumentNullException.ThrowIfNull(request);

        ServiceRequestAccessScope accessScope =
            GetRequiredAccessScope();

        if (!ServiceRequestAccessRules
                .CanManageAssignments(
                    accessScope.Role))
        {
            throw new ForbiddenException(
                "Only managers and administrators can assign service requests.");
        }

        ServiceRequest serviceRequest =
            await GetRequestForUpdateAsync(
                serviceRequestId,
                cancellationToken);

        if (!ServiceRequestRules.CanAssign(
                serviceRequest.Status))
        {
            throw new BusinessRuleException(
                $"A service request with status " +
                $"'{serviceRequest.Status}' cannot be assigned.");
        }

        AppUser? staffUser =
            await _userRepository.GetByIdAsync(
                request.StaffUserId,
                cancellationToken);

        if (staffUser is null
            || staffUser.Role != UserRole.Staff)
        {
            throw new BusinessRuleException(
                "The selected user does not exist, is inactive, " +
                "or is not a staff member.");
        }

        DateTimeOffset now =
            _timeProvider.GetUtcNow();

        ServiceRequestStatus previousStatus =
            serviceRequest.Status;

        serviceRequest.AssignedToUserId =
            staffUser.Id;

        serviceRequest.AssignedAt =
            now;

        serviceRequest.Status =
            ServiceRequestStatus.Assigned;

        serviceRequest.UpdatedAt =
            now;

        if (previousStatus ==
            ServiceRequestStatus.Reopened)
        {
            serviceRequest.ResolvedAt = null;
            serviceRequest.ClosedAt = null;
        }

        await AddStatusHistoryAsync(
            serviceRequest,
            previousStatus,
            ServiceRequestStatus.Assigned,
            accessScope.UserId,
            request.Note,
            now,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return await ReloadDetailsAsync(
            serviceRequest.Id,
            cancellationToken);
    }

    public async Task<ServiceRequestDetailsResponse>
        StartAsync(
            int serviceRequestId,
            StartServiceRequestRequest request,
            CancellationToken cancellationToken = default)
    {
        EnsureValidId(serviceRequestId);
        ArgumentNullException.ThrowIfNull(request);

        ServiceRequestAccessScope accessScope =
            GetRequiredAccessScope();

        if (accessScope.Role != UserRole.Staff)
        {
            throw new ForbiddenException(
                "Only staff members can start work on service requests.");
        }

        ServiceRequest serviceRequest =
            await GetRequestForUpdateAsync(
                serviceRequestId,
                cancellationToken);

        if (!ServiceRequestAccessRules.CanWorkOn(
                accessScope,
                serviceRequest))
        {
            throw new ForbiddenException(
                "You cannot start work on a service request " +
                "that is not assigned to you.");
        }

        if (!ServiceRequestRules.CanStart(
                serviceRequest.Status))
        {
            throw new BusinessRuleException(
                $"A service request with status " +
                $"'{serviceRequest.Status}' cannot be started.");
        }

        DateTimeOffset now =
            _timeProvider.GetUtcNow();

        ServiceRequestStatus previousStatus =
            serviceRequest.Status;

        serviceRequest.Status =
            ServiceRequestStatus.InProgress;

        serviceRequest.UpdatedAt =
            now;

        serviceRequest.ResolvedAt = null;
        serviceRequest.ClosedAt = null;

        await AddStatusHistoryAsync(
            serviceRequest,
            previousStatus,
            ServiceRequestStatus.InProgress,
            accessScope.UserId,
            request.Note,
            now,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return await ReloadDetailsAsync(
            serviceRequest.Id,
            cancellationToken);
    }

    public async Task<ServiceRequestDetailsResponse>
        ResolveAsync(
            int serviceRequestId,
            ResolveServiceRequestRequest request,
            CancellationToken cancellationToken = default)
    {
        EnsureValidId(serviceRequestId);
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(
                request.ResolutionNote))
        {
            throw new BusinessRuleException(
                "A resolution note is required.");
        }

        ServiceRequestAccessScope accessScope =
            GetRequiredAccessScope();

        if (accessScope.Role != UserRole.Staff)
        {
            throw new ForbiddenException(
                "Only staff members can resolve service requests.");
        }

        ServiceRequest serviceRequest =
            await GetRequestForUpdateAsync(
                serviceRequestId,
                cancellationToken);

        if (!ServiceRequestAccessRules.CanWorkOn(
                accessScope,
                serviceRequest))
        {
            throw new ForbiddenException(
                "You cannot resolve a service request " +
                "that is not assigned to you.");
        }

        if (!ServiceRequestRules.CanResolve(
                serviceRequest.Status))
        {
            throw new BusinessRuleException(
                $"A service request with status " +
                $"'{serviceRequest.Status}' cannot be resolved.");
        }

        DateTimeOffset now =
            _timeProvider.GetUtcNow();

        ServiceRequestStatus previousStatus =
            serviceRequest.Status;

        serviceRequest.Status =
            ServiceRequestStatus.Resolved;

        serviceRequest.ResolvedAt =
            now;

        serviceRequest.UpdatedAt =
            now;

        await AddStatusHistoryAsync(
            serviceRequest,
            previousStatus,
            ServiceRequestStatus.Resolved,
            accessScope.UserId,
            request.ResolutionNote,
            now,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return await ReloadDetailsAsync(
            serviceRequest.Id,
            cancellationToken);
    }

    private async Task<ServiceRequest>
        GetRequestForUpdateAsync(
            int serviceRequestId,
            CancellationToken cancellationToken)
    {
        ServiceRequest? serviceRequest =
            await _serviceRequestRepository
                .GetByIdForUpdateAsync(
                    serviceRequestId,
                    cancellationToken);

        if (serviceRequest is null)
        {
            throw new NotFoundException(
                "Service request",
                serviceRequestId);
        }

        return serviceRequest;
    }

    private async Task AddStatusHistoryAsync(
        ServiceRequest serviceRequest,
        ServiceRequestStatus previousStatus,
        ServiceRequestStatus newStatus,
        int changedByUserId,
        string? note,
        DateTimeOffset changedAt,
        CancellationToken cancellationToken)
    {
        string? normalizedNote =
            string.IsNullOrWhiteSpace(note)
                ? null
                : note.Trim();

        var statusHistory =
            new RequestStatusHistory
            {
                ServiceRequestId =
                    serviceRequest.Id,

                ServiceRequest =
                    serviceRequest,

                PreviousStatus =
                    previousStatus,

                NewStatus =
                    newStatus,

                ChangedByUserId =
                    changedByUserId,

                Note =
                    normalizedNote,

                CreatedAt =
                    changedAt
            };

        await _statusHistoryRepository.AddAsync(
            statusHistory,
            cancellationToken);
    }

    private async Task<ServiceRequestDetailsResponse>
        ReloadDetailsAsync(
            int serviceRequestId,
            CancellationToken cancellationToken)
    {
        ServiceRequest? updatedServiceRequest =
            await _serviceRequestRepository.GetByIdAsync(
                serviceRequestId,
                cancellationToken);

        if (updatedServiceRequest is null)
        {
            throw new NotFoundException(
                "Updated service request",
                serviceRequestId);
        }

        return ServiceRequestMapper.ToDetailsResponse(
            updatedServiceRequest);
    }

    private ServiceRequestAccessScope
        GetRequiredAccessScope()
    {
        if (!_currentUserService.IsAuthenticated
            || _currentUserService.UserId is null
            || _currentUserService.Role is null)
        {
            throw new UnauthenticatedException();
        }

        return new ServiceRequestAccessScope(
            _currentUserService.UserId.Value,
            _currentUserService.Role.Value);
    }

    private static void EnsureValidId(int id)
    {
        if (id < 1)
        {
            throw new BusinessRuleException(
                "The service request identifier must be greater than zero.");
        }
    }
}