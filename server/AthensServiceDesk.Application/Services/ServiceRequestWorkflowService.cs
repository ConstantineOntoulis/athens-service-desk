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
                    ServiceRequestStatus.Assigned,

                ChangedByUserId =
                    accessScope.UserId,

                Note =
                    string.IsNullOrWhiteSpace(
                        request.Note)
                        ? null
                        : request.Note.Trim(),

                CreatedAt =
                    now
            };

        await _statusHistoryRepository.AddAsync(
            statusHistory,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        ServiceRequest? updatedServiceRequest =
            await _serviceRequestRepository.GetByIdAsync(
                serviceRequest.Id,
                cancellationToken);

        if (updatedServiceRequest is null)
        {
            throw new NotFoundException(
                "Updated service request",
                serviceRequest.Id);
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