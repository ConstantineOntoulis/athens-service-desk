using AthensServiceDesk.Application.Common.Exceptions;
using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.DTOs.Common;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Application.Services;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Tests.Services;

public sealed class ServiceRequestServiceTests
{
    private readonly FakeServiceRequestRepository
        _serviceRequestRepository = new();

    private readonly FakeDepartmentRepository
        _departmentRepository = new();

    private readonly FakeServiceCategoryRepository
        _serviceCategoryRepository = new();

    private readonly FakeUnitOfWork
        _unitOfWork = new();

    private readonly FakeCurrentUserService
        _currentUserService = new();

    private readonly ServiceRequestService _service;

    public ServiceRequestServiceTests()
    {
        _currentUserService.IsAuthenticated = true;
        _currentUserService.UserId = 17;
        _currentUserService.Role = UserRole.Citizen;
        _currentUserService.Email =
            "citizen@athensdesk.local";

        _service = new ServiceRequestService(
            _serviceRequestRepository,
            _departmentRepository,
            _serviceCategoryRepository,
            _unitOfWork,
            _currentUserService);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldNormalizeQueryAndReturnPagedResponse()
    {
        // Arrange
        _serviceRequestRepository.ListResult =
        [
            CreateServiceRequestEntity()
        ];

        _serviceRequestRepository.CountResult = 12;

        var query = new ServiceRequestQuery
        {
            Page = -5,
            PageSize = 500,
            Search = "  streetlight  ",
            SortBy = "invalid-field",
            SortDirection = "asc"
        };

        // Act
        PagedResponse<ServiceRequestResponse> result =
            await _service.GetPagedAsync(query);

        // Assert
        Assert.Equal(1, result.Page);
        Assert.Equal(50, result.PageSize);
        Assert.Equal(12, result.TotalCount);
        Assert.Equal(1, result.TotalPages);

        ServiceRequestResponse item =
            Assert.Single(result.Items);

        Assert.Equal(1, item.Id);
        Assert.Equal(
            "Broken streetlight",
            item.Title);

        Assert.Equal(
            "Infrastructure",
            item.DepartmentName);

        Assert.Equal(
            "Streetlight Issue",
            item.ServiceCategoryName);

        ServiceRequestQuery normalizedQuery =
            Assert.IsType<ServiceRequestQuery>(
                _serviceRequestRepository.LastListQuery);

        Assert.Equal(1, normalizedQuery.Page);
        Assert.Equal(50, normalizedQuery.PageSize);
        Assert.Equal(
            "streetlight",
            normalizedQuery.Search);

        Assert.Equal(
            "createdAt",
            normalizedQuery.SortBy);

        Assert.Equal(
            "asc",
            normalizedQuery.SortDirection);

        Assert.Same(
            _serviceRequestRepository.LastListQuery,
            _serviceRequestRepository.LastCountQuery);

        ServiceRequestAccessScope listAccessScope =
            Assert.IsType<ServiceRequestAccessScope>(
                _serviceRequestRepository
                    .LastListAccessScope);

        ServiceRequestAccessScope countAccessScope =
            Assert.IsType<ServiceRequestAccessScope>(
                _serviceRequestRepository
                    .LastCountAccessScope);

        Assert.Equal(
            17,
            listAccessScope.UserId);

        Assert.Equal(
            UserRole.Citizen,
            listAccessScope.Role);

        Assert.Same(
            _serviceRequestRepository
                .LastListAccessScope,
            _serviceRequestRepository
                .LastCountAccessScope);

        Assert.Equal(
            listAccessScope,
            countAccessScope);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldThrowUnauthenticatedException_WhenCurrentUserIsMissing()
    {
        // Arrange
        _currentUserService.IsAuthenticated = false;
        _currentUserService.UserId = null;
        _currentUserService.Role = null;

        var query = new ServiceRequestQuery();

        // Act
        await Assert.ThrowsAsync<
            UnauthenticatedException>(
                () => _service.GetPagedAsync(query));

        // Assert
        Assert.Null(
            _serviceRequestRepository.LastListQuery);

        Assert.Null(
            _serviceRequestRepository.LastCountQuery);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMappedDetails_WhenRequestExistsAndUserHasAccess()
    {
        // Arrange
        _serviceRequestRepository.ReadResult =
            CreateServiceRequestEntity(id: 7);

        // Act
        ServiceRequestDetailsResponse result =
            await _service.GetByIdAsync(7);

        // Assert
        Assert.Equal(7, result.Id);

        Assert.Equal(
            "Broken streetlight",
            result.Title);

        Assert.Equal(
            "Infrastructure",
            result.DepartmentName);

        Assert.Equal(
            "Streetlight Issue",
            result.ServiceCategoryName);

        Assert.Equal(
            "Submitted",
            result.Status);

        Assert.Equal(
            "High",
            result.Priority);

        Assert.Equal(
            17,
            result.CreatedByUserId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenRequestDoesNotExist()
    {
        // Arrange
        _serviceRequestRepository.ReadResult = null;

        // Act
        NotFoundException exception =
            await Assert.ThrowsAsync<
                NotFoundException>(
                    () => _service.GetByIdAsync(99));

        // Assert
        Assert.Contains(
            "99",
            exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowBusinessRuleException_WhenIdIsNotPositive()
    {
        // Act
        BusinessRuleException exception =
            await Assert.ThrowsAsync<
                BusinessRuleException>(
                    () => _service.GetByIdAsync(0));

        // Assert
        Assert.Contains(
            "greater than zero",
            exception.Message);

        Assert.Equal(
            0,
            _serviceRequestRepository
                .GetByIdCallCount);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowForbiddenException_WhenCitizenDoesNotOwnRequest()
    {
        // Arrange
        ServiceRequest serviceRequest =
            CreateServiceRequestEntity(id: 7);

        serviceRequest.CreatedByUserId = 99;

        _serviceRequestRepository.ReadResult =
            serviceRequest;

        // Act
        ForbiddenException exception =
            await Assert.ThrowsAsync<
                ForbiddenException>(
                    () => _service.GetByIdAsync(7));

        // Assert
        Assert.Contains(
            "permission",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectRequest_WhenDepartmentDoesNotExist()
    {
        // Arrange
        _departmentRepository.ExistsResult = false;

        CreateServiceRequestRequest request =
            CreateValidCreateRequest();

        // Act
        BusinessRuleException exception =
            await Assert.ThrowsAsync<
                BusinessRuleException>(
                    () => _service.CreateAsync(request));

        // Assert
        Assert.Contains(
            "department",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Null(
            _serviceRequestRepository.AddedEntity);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectRequest_WhenCategoryDoesNotExist()
    {
        // Arrange
        _departmentRepository.ExistsResult = true;

        _serviceCategoryRepository
            .DepartmentIdResult = null;

        CreateServiceRequestRequest request =
            CreateValidCreateRequest();

        // Act
        BusinessRuleException exception =
            await Assert.ThrowsAsync<
                BusinessRuleException>(
                    () => _service.CreateAsync(request));

        // Assert
        Assert.Contains(
            "category",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Null(
            _serviceRequestRepository.AddedEntity);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectRequest_WhenCategoryBelongsToDifferentDepartment()
    {
        // Arrange
        _departmentRepository.ExistsResult = true;

        // The request selects department 2,
        // but the category belongs to department 4.
        _serviceCategoryRepository
            .DepartmentIdResult = 4;

        CreateServiceRequestRequest request =
            CreateValidCreateRequest();

        // Act
        BusinessRuleException exception =
            await Assert.ThrowsAsync<
                BusinessRuleException>(
                    () => _service.CreateAsync(request));

        // Assert
        Assert.Contains(
            "does not belong",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Null(
            _serviceRequestRepository.AddedEntity);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimInputAssignCurrentUserForceSubmittedStatusAndSaveOnce()
    {
        // Arrange
        _departmentRepository.ExistsResult = true;

        _serviceCategoryRepository
            .DepartmentIdResult = 2;

        _serviceRequestRepository.ReadResult =
            CreateServiceRequestEntity(id: 42);

        var request =
            new CreateServiceRequestRequest
            {
                Title =
                    "  Broken streetlight  ",

                Description =
                    "  The streetlight has not worked for several nights.  ",

                Location =
                    "  Syntagma Square  ",

                DepartmentId = 2,

                ServiceCategoryId = 3,

                Priority =
                    ServicePriority.High
            };

        // Act
        ServiceRequestDetailsResponse result =
            await _service.CreateAsync(request);

        // Assert
        ServiceRequest addedEntity =
            Assert.IsType<ServiceRequest>(
                _serviceRequestRepository
                    .AddedEntity);

        Assert.Equal(
            42,
            addedEntity.Id);

        Assert.Equal(
            "Broken streetlight",
            addedEntity.Title);

        Assert.Equal(
            "The streetlight has not worked for several nights.",
            addedEntity.Description);

        Assert.Equal(
            "Syntagma Square",
            addedEntity.Location);

        Assert.Equal(
            2,
            addedEntity.DepartmentId);

        Assert.Equal(
            3,
            addedEntity.ServiceCategoryId);

        Assert.Equal(
            ServicePriority.High,
            addedEntity.Priority);

        Assert.Equal(
            ServiceRequestStatus.Submitted,
            addedEntity.Status);

        Assert.Equal(
            17,
            addedEntity.CreatedByUserId);

        Assert.Equal(
            1,
            _unitOfWork.SaveCallCount);

        Assert.Equal(
            42,
            _serviceRequestRepository
                .LastRequestedId);

        Assert.Equal(
            42,
            result.Id);

        Assert.Equal(
            17,
            result.CreatedByUserId);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowForbiddenException_ForStaff()
    {
        // Arrange
        _currentUserService.Role =
            UserRole.Staff;

        CreateServiceRequestRequest request =
            CreateValidCreateRequest();

        // Act
        ForbiddenException exception =
            await Assert.ThrowsAsync<
                ForbiddenException>(
                    () => _service.CreateAsync(request));

        // Assert
        Assert.Contains(
            "citizens",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Null(
            _serviceRequestRepository.AddedEntity);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowUnauthenticatedException_WhenCurrentUserCannotBeResolved()
    {
        // Arrange
        _currentUserService.IsAuthenticated = false;
        _currentUserService.UserId = null;
        _currentUserService.Role = null;

        CreateServiceRequestRequest request =
            CreateValidCreateRequest();

        // Act
        await Assert.ThrowsAsync<
            UnauthenticatedException>(
                () => _service.CreateAsync(request));

        // Assert
        Assert.Null(
            _serviceRequestRepository.AddedEntity);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenRequestDoesNotExist()
    {
        // Arrange
        _serviceRequestRepository.TrackedResult =
            null;

        UpdateServiceRequestRequest request =
            CreateValidUpdateRequest();

        // Act
        NotFoundException exception =
            await Assert.ThrowsAsync<
                NotFoundException>(
                    () => _service.UpdateAsync(
                        50,
                        request));

        // Assert
        Assert.Contains(
            "50",
            exception.Message);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForbiddenException_WhenCitizenDoesNotOwnRequest()
    {
        // Arrange
        ServiceRequest serviceRequest =
            CreateServiceRequestEntity(
                id: 10,
                status:
                    ServiceRequestStatus.Submitted);

        serviceRequest.CreatedByUserId = 99;

        _serviceRequestRepository.TrackedResult =
            serviceRequest;

        UpdateServiceRequestRequest request =
            CreateValidUpdateRequest();

        // Act
        ForbiddenException exception =
            await Assert.ThrowsAsync<
                ForbiddenException>(
                    () => _service.UpdateAsync(
                        10,
                        request));

        // Assert
        Assert.Contains(
            "permission",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectRequest_WhenCurrentStatusCannotBeEdited()
    {
        // Arrange
        _serviceRequestRepository.TrackedResult =
            CreateServiceRequestEntity(
                id: 10,
                status:
                    ServiceRequestStatus.Closed);

        UpdateServiceRequestRequest request =
            CreateValidUpdateRequest();

        // Act
        BusinessRuleException exception =
            await Assert.ThrowsAsync<
                BusinessRuleException>(
                    () => _service.UpdateAsync(
                        10,
                        request));

        // Assert
        Assert.Contains(
            "cannot be edited",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(
            0,
            _unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAllowedPropertiesAndSaveOnce()
    {
        // Arrange
        ServiceRequest trackedEntity =
            CreateServiceRequestEntity(
                id: 10,
                status:
                    ServiceRequestStatus.Submitted);

        DateTimeOffset originalCreatedAt =
            trackedEntity.CreatedAt;

        _serviceRequestRepository.TrackedResult =
            trackedEntity;

        _serviceRequestRepository.ReadResult =
            trackedEntity;

        _departmentRepository.ExistsResult = true;

        _serviceCategoryRepository
            .DepartmentIdResult = 4;

        var request =
            new UpdateServiceRequestRequest
            {
                Title =
                    "  Damaged public bench  ",

                Description =
                    "  The public bench has a broken seat and exposed metal.  ",

                Location =
                    "  National Garden  ",

                DepartmentId = 4,

                ServiceCategoryId = 9,

                Priority =
                    ServicePriority.Urgent
            };

        // Act
        ServiceRequestDetailsResponse result =
            await _service.UpdateAsync(
                10,
                request);

        // Assert
        Assert.Equal(
            "Damaged public bench",
            trackedEntity.Title);

        Assert.Equal(
            "The public bench has a broken seat and exposed metal.",
            trackedEntity.Description);

        Assert.Equal(
            "National Garden",
            trackedEntity.Location);

        Assert.Equal(
            4,
            trackedEntity.DepartmentId);

        Assert.Equal(
            9,
            trackedEntity.ServiceCategoryId);

        Assert.Equal(
            ServicePriority.Urgent,
            trackedEntity.Priority);

        // UpdateAsync must not modify
        // workflow-controlled values.
        Assert.Equal(
            ServiceRequestStatus.Submitted,
            trackedEntity.Status);

        Assert.Equal(
            originalCreatedAt,
            trackedEntity.CreatedAt);

        Assert.Equal(
            17,
            trackedEntity.CreatedByUserId);

        Assert.NotNull(
            trackedEntity.UpdatedAt);

        Assert.Equal(
            1,
            _unitOfWork.SaveCallCount);

        Assert.Equal(
            10,
            result.Id);

        Assert.Equal(
            "Damaged public bench",
            result.Title);

        Assert.Equal(
            "Urgent",
            result.Priority);
    }

    private static CreateServiceRequestRequest
        CreateValidCreateRequest()
    {
        return new CreateServiceRequestRequest
        {
            Title =
                "Broken streetlight",

            Description =
                "The streetlight has not worked for several nights.",

            Location =
                "Syntagma Square",

            DepartmentId = 2,

            ServiceCategoryId = 3,

            Priority =
                ServicePriority.High
        };
    }

    private static UpdateServiceRequestRequest
        CreateValidUpdateRequest()
    {
        return new UpdateServiceRequestRequest
        {
            Title =
                "Updated streetlight report",

            Description =
                "The streetlight remains broken after the initial report.",

            Location =
                "Syntagma Square",

            DepartmentId = 2,

            ServiceCategoryId = 3,

            Priority =
                ServicePriority.High
        };
    }

    private static ServiceRequest
        CreateServiceRequestEntity(
            int id = 1,
            ServiceRequestStatus status =
                ServiceRequestStatus.Submitted)
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
                DepartmentId =
                    department.Id,

                Department =
                    department,

                IsActive = true
            };

        return new ServiceRequest
        {
            Id = id,

            Title =
                "Broken streetlight",

            Description =
                "The streetlight has not worked for several nights.",

            Location =
                "Syntagma Square",

            Status = status,

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

            CreatedByUserId = 17,

            CreatedAt =
                new DateTimeOffset(
                    2026,
                    6,
                    1,
                    12,
                    0,
                    0,
                    TimeSpan.Zero)
        };
    }

    private sealed class FakeServiceRequestRepository
        : IServiceRequestRepository
    {
        public ServiceRequest? ReadResult
        {
            get;
            set;
        }

        public ServiceRequest? TrackedResult
        {
            get;
            set;
        }

        public IReadOnlyList<ServiceRequest>
            ListResult
        {
            get;
            set;
        } = [];

        public int CountResult
        {
            get;
            set;
        }

        public ServiceRequest? AddedEntity
        {
            get;
            private set;
        }

        public ServiceRequestQuery? LastListQuery
        {
            get;
            private set;
        }

        public ServiceRequestQuery? LastCountQuery
        {
            get;
            private set;
        }

        public ServiceRequestAccessScope?
            LastListAccessScope
        {
            get;
            private set;
        }

        public ServiceRequestAccessScope?
            LastCountAccessScope
        {
            get;
            private set;
        }

        public int? LastRequestedId
        {
            get;
            private set;
        }

        public int GetByIdCallCount
        {
            get;
            private set;
        }

        public Task<ServiceRequest?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            LastRequestedId = id;
            GetByIdCallCount++;

            return Task.FromResult(
                ReadResult);
        }

        public Task<ServiceRequest?>
            GetByIdForUpdateAsync(
                int id,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                TrackedResult);
        }

        public Task<IReadOnlyList<ServiceRequest>>
            ListAsync(
                ServiceRequestQuery query,
                ServiceRequestAccessScope accessScope,
                CancellationToken cancellationToken = default)
        {
            LastListQuery = query;

            LastListAccessScope =
                accessScope;

            return Task.FromResult(
                ListResult);
        }

        public Task<int> CountAsync(
            ServiceRequestQuery query,
            ServiceRequestAccessScope accessScope,
            CancellationToken cancellationToken = default)
        {
            LastCountQuery = query;

            LastCountAccessScope =
                accessScope;

            return Task.FromResult(
                CountResult);
        }

        public Task AddAsync(
            ServiceRequest serviceRequest,
            CancellationToken cancellationToken = default)
        {
            // Simulates SQL Server assigning
            // a generated identity value.
            if (serviceRequest.Id == 0)
            {
                serviceRequest.Id = 42;
            }

            AddedEntity =
                serviceRequest;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeDepartmentRepository
        : IDepartmentRepository
    {
        public bool ExistsResult
        {
            get;
            set;
        } = true;

        public IReadOnlyList<Department>
            ActiveDepartments
        {
            get;
            set;
        } = [];

        public Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ExistsResult);
        }

        public Task<IReadOnlyList<Department>>
            GetActiveAsync(
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ActiveDepartments);
        }
    }

    private sealed class FakeServiceCategoryRepository
        : IServiceCategoryRepository
    {
        public bool ExistsResult
        {
            get;
            set;
        } = true;

        public int? DepartmentIdResult
        {
            get;
            set;
        } = 2;

        public IReadOnlyList<ServiceCategory>
            ActiveCategories
        {
            get;
            set;
        } = [];

        public Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ExistsResult);
        }

        public Task<int?> GetDepartmentIdAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                DepartmentIdResult);
        }

        public Task<IReadOnlyList<ServiceCategory>>
            GetActiveByDepartmentIdAsync(
                int departmentId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ActiveCategories);
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
        public bool IsAuthenticated
        {
            get;
            set;
        }

        public int? UserId
        {
            get;
            set;
        }

        public string? Email
        {
            get;
            set;
        }

        public string? FirstName
        {
            get;
            set;
        }

        public string? LastName
        {
            get;
            set;
        }

        public UserRole? Role
        {
            get;
            set;
        }
    }
}