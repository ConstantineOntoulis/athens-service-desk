using AthensServiceDesk.Application.Common.Exceptions;
using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Application.Services;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Tests.Services;

public sealed class ServiceRequestWorkflowServiceTests
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(
            2026,
            7,
            1,
            12,
            0,
            0,
            TimeSpan.Zero);

    private readonly FakeServiceRequestRepository
        _serviceRequestRepository = new();

    private readonly FakeRequestStatusHistoryRepository
        _historyRepository = new();

    private readonly FakeUserRepository
        _userRepository = new();

    private readonly FakeUnitOfWork
        _unitOfWork = new();

    private readonly FakeCurrentUserService
        _currentUserService = new();

    private readonly ServiceRequestWorkflowService
        _service;

    public ServiceRequestWorkflowServiceTests()
    {
        _currentUserService.IsAuthenticated = true;
        _currentUserService.UserId = 30;
        _currentUserService.Role = UserRole.Manager;

        _service =
            new ServiceRequestWorkflowService(
                _serviceRequestRepository,
                _historyRepository,
                _userRepository,
                _unitOfWork,
                _currentUserService,
                new FixedTimeProvider(FixedUtcNow));
    }

    [Fact]
    public async Task AssignAsync_ShouldAssignStaffChangeStatusAndRecordHistory()
    {
        ServiceRequest serviceRequest =
            CreateServiceRequest();

        _serviceRequestRepository.TrackedResult =
            serviceRequest;

        _serviceRequestRepository.ReadResult =
            serviceRequest;

        _userRepository.IdResult =
            CreateStaffUser();

        var request =
            new AssignServiceRequestRequest
            {
                StaffUserId = 20,
                Note = "  Assigned to infrastructure staff.  "
            };

        ServiceRequestDetailsResponse result =
            await _service.AssignAsync(
                10,
                request);

        Assert.Equal(
            20,
            serviceRequest.AssignedToUserId);

        Assert.Equal(
            ServiceRequestStatus.Assigned,
            serviceRequest.Status);

        Assert.Equal(
            FixedUtcNow,
            serviceRequest.AssignedAt);

        Assert.Equal(
            FixedUtcNow,
            serviceRequest.UpdatedAt);

        RequestStatusHistory history =
            Assert.IsType<RequestStatusHistory>(
                _historyRepository.AddedEntity);

        Assert.Equal(
            ServiceRequestStatus.Submitted,
            history.PreviousStatus);

        Assert.Equal(
            ServiceRequestStatus.Assigned,
            history.NewStatus);

        Assert.Equal(
            30,
            history.ChangedByUserId);

        Assert.Equal(
            "Assigned to infrastructure staff.",
            history.Note);

        Assert.Equal(
            FixedUtcNow,
            history.CreatedAt);

        Assert.Equal(
            1,
            _unitOfWork.SaveCallCount);

        Assert.Equal(
            "Assigned",
            result.Status);

        Assert.Equal(
            20,
            result.AssignedToUserId);

        RequestStatusHistoryResponse responseHistory =
            Assert.Single(result.StatusHistory);

        Assert.Equal(
            "Submitted",
            responseHistory.PreviousStatus);

        Assert.Equal(
            "Assigned",
            responseHistory.NewStatus);
    }

    [Fact]
    public async Task AssignAsync_ShouldThrowForbidden_ForCitizen()
    {
        _currentUserService.Role =
            UserRole.Citizen;

        var request =
            new AssignServiceRequestRequest
            {
                StaffUserId = 20
            };

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.AssignAsync(
                10,
                request));

        Assert.Equal(
            0,
            _serviceRequestRepository
                .GetForUpdateCallCount);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task AssignAsync_ShouldThrowNotFound_WhenRequestDoesNotExist()
    {
        _serviceRequestRepository.TrackedResult =
            null;

        var request =
            new AssignServiceRequestRequest
            {
                StaffUserId = 20
            };

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.AssignAsync(
                999,
                request));

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task AssignAsync_ShouldRejectRequest_WhenStatusCannotBeAssigned()
    {
        ServiceRequest serviceRequest =
            CreateServiceRequest();

        serviceRequest.Status =
            ServiceRequestStatus.Closed;

        _serviceRequestRepository.TrackedResult =
            serviceRequest;

        var request =
            new AssignServiceRequestRequest
            {
                StaffUserId = 20
            };

        BusinessRuleException exception =
            await Assert.ThrowsAsync<
                BusinessRuleException>(
                    () => _service.AssignAsync(
                        10,
                        request));

        Assert.Contains(
            "cannot be assigned",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task AssignAsync_ShouldRejectTarget_WhenUserIsNotStaff()
    {
        _serviceRequestRepository.TrackedResult =
            CreateServiceRequest();

        _userRepository.IdResult =
            new AppUser
            {
                Id = 40,
                Email = "manager@example.com",
                Role = UserRole.Manager,
                IsActive = true
            };

        var request =
            new AssignServiceRequestRequest
            {
                StaffUserId = 40
            };

        BusinessRuleException exception =
            await Assert.ThrowsAsync<
                BusinessRuleException>(
                    () => _service.AssignAsync(
                        10,
                        request));

        Assert.Contains(
            "not a staff member",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    private static ServiceRequest
        CreateServiceRequest()
    {
        var department =
            new Department
            {
                Id = 2,
                Name = "Infrastructure",
                IsActive = true
            };

        var category =
            new ServiceCategory
            {
                Id = 3,
                Name = "Streetlight Issue",
                DepartmentId = 2,
                Department = department,
                IsActive = true
            };

        return new ServiceRequest
        {
            Id = 10,
            Title = "Broken streetlight",
            Description =
                "The streetlight is not working.",
            Location = "Athens",
            Status =
                ServiceRequestStatus.Submitted,
            Priority =
                ServicePriority.High,
            DepartmentId = 2,
            Department = department,
            ServiceCategoryId = 3,
            ServiceCategory = category,
            CreatedByUserId = 5,
            CreatedAt =
                new DateTimeOffset(
                    2026,
                    6,
                    30,
                    12,
                    0,
                    0,
                    TimeSpan.Zero)
        };
    }

    private static AppUser CreateStaffUser()
    {
        return new AppUser
        {
            Id = 20,
            Email = "staff@example.com",
            FirstName = "Demo",
            LastName = "Staff",
            Role = UserRole.Staff,
            IsActive = true
        };
    }

    private sealed class FakeServiceRequestRepository
        : IServiceRequestRepository
    {
        public ServiceRequest? ReadResult { get; set; }

        public ServiceRequest? TrackedResult { get; set; }

        public int GetForUpdateCallCount
        {
            get;
            private set;
        }

        public Task<ServiceRequest?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ReadResult);
        }

        public Task<ServiceRequest?>
            GetByIdForUpdateAsync(
                int id,
                CancellationToken cancellationToken = default)
        {
            GetForUpdateCallCount++;

            return Task.FromResult(TrackedResult);
        }

        public Task<IReadOnlyList<ServiceRequest>>
            ListAsync(
                ServiceRequestQuery query,
                ServiceRequestAccessScope accessScope,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<
                IReadOnlyList<ServiceRequest>>([]);
        }

        public Task<int> CountAsync(
            ServiceRequestQuery query,
            ServiceRequestAccessScope accessScope,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task AddAsync(
            ServiceRequest serviceRequest,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRequestStatusHistoryRepository
        : IRequestStatusHistoryRepository
    {
        public RequestStatusHistory? AddedEntity
        {
            get;
            private set;
        }

        public Task AddAsync(
            RequestStatusHistory statusHistory,
            CancellationToken cancellationToken = default)
        {
            statusHistory.Id = 1;

            statusHistory.ChangedByUser =
                new AppUser
                {
                    Id = statusHistory.ChangedByUserId,
                    FirstName = "Demo",
                    LastName = "Manager",
                    Role = UserRole.Manager
                };

            statusHistory.ServiceRequest
                .StatusHistory.Add(statusHistory);

            AddedEntity = statusHistory;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserRepository
        : IUserRepository
    {
        public AppUser? IdResult { get; set; }

        public Task<AppUser?>
            GetByNormalizedEmailAsync(
                string normalizedEmail,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AppUser?>(null);
        }

        public Task<AppUser?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IdResult);
        }
    }

    private sealed class FakeUnitOfWork
        : IUnitOfWork
    {
        public int SaveCallCount
        {
            get;
            private set;
        }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCallCount++;

            return Task.FromResult(1);
        }
    }

    private sealed class FakeCurrentUserService
        : ICurrentUserService
    {
        public bool IsAuthenticated { get; set; }

        public int? UserId { get; set; }

        public string? Email { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public UserRole? Role { get; set; }
    }

    private sealed class FixedTimeProvider(
        DateTimeOffset utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}