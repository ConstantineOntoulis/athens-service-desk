using AthensServiceDesk.Application.Common.Exceptions;
using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Application.Services;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Tests.Services;

public sealed class ServiceRequestWorkProgressionServiceTests
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(
            2026,
            7,
            2,
            12,
            0,
            0,
            TimeSpan.Zero);

    private readonly FakeServiceRequestRepository
        _serviceRequestRepository = new();

    private readonly FakeStatusHistoryRepository
        _statusHistoryRepository = new();

    private readonly FakeUserRepository
        _userRepository = new();

    private readonly FakeUnitOfWork
        _unitOfWork = new();

    private readonly FakeCurrentUserService
        _currentUserService = new();

    private readonly ServiceRequestWorkflowService
        _service;

    public ServiceRequestWorkProgressionServiceTests()
    {
        _currentUserService.IsAuthenticated = true;
        _currentUserService.UserId = 20;
        _currentUserService.Role = UserRole.Staff;

        _service =
            new ServiceRequestWorkflowService(
                _serviceRequestRepository,
                _statusHistoryRepository,
                _userRepository,
                _unitOfWork,
                _currentUserService,
                new FixedTimeProvider(FixedUtcNow));
    }

    [Fact]
    public async Task StartAsync_ShouldChangeAssignedRequestToInProgress()
    {
        ServiceRequest serviceRequest =
            CreateServiceRequest(
                ServiceRequestStatus.Assigned,
                assignedToUserId: 20);

        _serviceRequestRepository.TrackedResult =
            serviceRequest;

        _serviceRequestRepository.ReadResult =
            serviceRequest;

        var request =
            new StartServiceRequestRequest
            {
                Note = "  Inspection started.  "
            };

        ServiceRequestDetailsResponse result =
            await _service.StartAsync(
                10,
                request);

        Assert.Equal(
            ServiceRequestStatus.InProgress,
            serviceRequest.Status);

        Assert.Equal(
            FixedUtcNow,
            serviceRequest.UpdatedAt);

        RequestStatusHistory history =
            Assert.IsType<RequestStatusHistory>(
                _statusHistoryRepository.AddedEntity);

        Assert.Equal(
            ServiceRequestStatus.Assigned,
            history.PreviousStatus);

        Assert.Equal(
            ServiceRequestStatus.InProgress,
            history.NewStatus);

        Assert.Equal(
            20,
            history.ChangedByUserId);

        Assert.Equal(
            "Inspection started.",
            history.Note);

        Assert.Equal(
            1,
            _unitOfWork.SaveCallCount);

        Assert.Equal(
            "InProgress",
            result.Status);

        RequestStatusHistoryResponse responseHistory =
            Assert.Single(result.StatusHistory);

        Assert.Equal(
            "Assigned",
            responseHistory.PreviousStatus);

        Assert.Equal(
            "InProgress",
            responseHistory.NewStatus);
    }

    [Fact]
    public async Task StartAsync_ShouldThrowForbidden_WhenRequestIsAssignedToDifferentStaff()
    {
        _serviceRequestRepository.TrackedResult =
            CreateServiceRequest(
                ServiceRequestStatus.Assigned,
                assignedToUserId: 25);

        var request =
            new StartServiceRequestRequest();

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.StartAsync(
                10,
                request));

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task StartAsync_ShouldRejectInvalidCurrentStatus()
    {
        _serviceRequestRepository.TrackedResult =
            CreateServiceRequest(
                ServiceRequestStatus.Submitted,
                assignedToUserId: 20);

        var request =
            new StartServiceRequestRequest();

        BusinessRuleException exception =
            await Assert.ThrowsAsync<
                BusinessRuleException>(
                    () => _service.StartAsync(
                        10,
                        request));

        Assert.Contains(
            "cannot be started",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task ResolveAsync_ShouldChangeInProgressRequestToResolved()
    {
        ServiceRequest serviceRequest =
            CreateServiceRequest(
                ServiceRequestStatus.InProgress,
                assignedToUserId: 20);

        _serviceRequestRepository.TrackedResult =
            serviceRequest;

        _serviceRequestRepository.ReadResult =
            serviceRequest;

        var request =
            new ResolveServiceRequestRequest
            {
                ResolutionNote =
                    "  The faulty lamp was replaced and tested.  "
            };

        ServiceRequestDetailsResponse result =
            await _service.ResolveAsync(
                10,
                request);

        Assert.Equal(
            ServiceRequestStatus.Resolved,
            serviceRequest.Status);

        Assert.Equal(
            FixedUtcNow,
            serviceRequest.ResolvedAt);

        Assert.Equal(
            FixedUtcNow,
            serviceRequest.UpdatedAt);

        RequestStatusHistory history =
            Assert.IsType<RequestStatusHistory>(
                _statusHistoryRepository.AddedEntity);

        Assert.Equal(
            ServiceRequestStatus.InProgress,
            history.PreviousStatus);

        Assert.Equal(
            ServiceRequestStatus.Resolved,
            history.NewStatus);

        Assert.Equal(
            "The faulty lamp was replaced and tested.",
            history.Note);

        Assert.Equal(
            1,
            _unitOfWork.SaveCallCount);

        Assert.Equal(
            "Resolved",
            result.Status);

        Assert.Equal(
            FixedUtcNow,
            result.ResolvedAt);
    }

    [Fact]
    public async Task ResolveAsync_ShouldRejectRequestThatIsNotInProgress()
    {
        _serviceRequestRepository.TrackedResult =
            CreateServiceRequest(
                ServiceRequestStatus.Assigned,
                assignedToUserId: 20);

        var request =
            new ResolveServiceRequestRequest
            {
                ResolutionNote =
                    "The issue has been completely resolved."
            };

        BusinessRuleException exception =
            await Assert.ThrowsAsync<
                BusinessRuleException>(
                    () => _service.ResolveAsync(
                        10,
                        request));

        Assert.Contains(
            "cannot be resolved",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task ResolveAsync_ShouldRejectWhitespaceResolutionNote()
    {
        var request =
            new ResolveServiceRequestRequest
            {
                ResolutionNote = "   "
            };

        await Assert.ThrowsAsync<
            BusinessRuleException>(
                () => _service.ResolveAsync(
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

    private static ServiceRequest
        CreateServiceRequest(
            ServiceRequestStatus status,
            int assignedToUserId)
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

            Title =
                "Broken streetlight",

            Description =
                "The streetlight is not working.",

            Location =
                "Athens",

            Status =
                status,

            Priority =
                ServicePriority.High,

            DepartmentId =
                department.Id,

            Department =
                department,

            ServiceCategoryId =
                category.Id,

            ServiceCategory =
                category,

            CreatedByUserId =
                5,

            AssignedToUserId =
                assignedToUserId,

            AssignedAt =
                new DateTimeOffset(
                    2026,
                    7,
                    1,
                    10,
                    0,
                    0,
                    TimeSpan.Zero),

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
            return Task.FromResult(
                ReadResult);
        }

        public Task<ServiceRequest?>
            GetByIdForUpdateAsync(
                int id,
                CancellationToken cancellationToken = default)
        {
            GetForUpdateCallCount++;

            return Task.FromResult(
                TrackedResult);
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

    private sealed class FakeStatusHistoryRepository
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
            statusHistory.Id =
                statusHistory.ServiceRequest
                    .StatusHistory.Count + 1;

            statusHistory.ChangedByUser =
                new AppUser
                {
                    Id =
                        statusHistory.ChangedByUserId,

                    FirstName =
                        "Demo",

                    LastName =
                        "Staff",

                    Role =
                        UserRole.Staff
                };

            statusHistory.ServiceRequest
                .StatusHistory.Add(
                    statusHistory);

            AddedEntity =
                statusHistory;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserRepository
        : IUserRepository
    {
        public Task<AppUser?>
            GetByNormalizedEmailAsync(
                string normalizedEmail,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AppUser?>(
                null);
        }

        public Task<AppUser?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AppUser?>(
                null);
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

    private sealed class FixedTimeProvider
        : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(
            DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}