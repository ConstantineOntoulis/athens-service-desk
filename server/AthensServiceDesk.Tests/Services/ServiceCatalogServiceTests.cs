using AthensServiceDesk.Application.Common.Exceptions;
using AthensServiceDesk.Application.DTOs.Lookups;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Application.Services;
using AthensServiceDesk.Domain.Entities;

namespace AthensServiceDesk.Tests.Services;

public sealed class ServiceCatalogServiceTests
{
    private readonly FakeDepartmentRepository _departmentRepository =
        new();

    private readonly FakeServiceCategoryRepository
        _serviceCategoryRepository = new();

    private readonly ServiceCatalogService _service;

    public ServiceCatalogServiceTests()
    {
        _service = new ServiceCatalogService(
            _departmentRepository,
            _serviceCategoryRepository);
    }

    [Fact]
    public async Task GetDepartmentsAsync_ShouldMapActiveDepartments()
    {
        // Arrange
        _departmentRepository.ActiveDepartments =
        [
            new Department
            {
                Id = 1,
                Name = "Citizen Services",
                Description = "Citizen-facing support.",
                IsActive = true
            },
            new Department
            {
                Id = 2,
                Name = "Infrastructure",
                Description = "Infrastructure-related requests.",
                IsActive = true
            }
        ];

        // Act
        IReadOnlyList<DepartmentResponse> result =
            await _service.GetDepartmentsAsync();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("Citizen Services", result[0].Name);
        Assert.Equal(
            "Citizen-facing support.",
            result[0].Description);

        Assert.Equal(2, result[1].Id);
        Assert.Equal("Infrastructure", result[1].Name);

        Assert.Equal(
            1,
            _departmentRepository.GetActiveCallCount);
    }

    [Fact]
    public async Task GetServiceCategoriesByDepartmentIdAsync_ShouldMapCategories()
    {
        // Arrange
        _departmentRepository.ExistsResult = true;

        _serviceCategoryRepository.ActiveCategories =
        [
            new ServiceCategory
            {
                Id = 4,
                Name = "Road Damage",
                Description = "Damaged road reports.",
                DepartmentId = 2,
                IsActive = true
            },
            new ServiceCategory
            {
                Id = 3,
                Name = "Streetlight Issue",
                Description = "Street lighting reports.",
                DepartmentId = 2,
                IsActive = true
            }
        ];

        // Act
        IReadOnlyList<ServiceCategoryResponse> result =
            await _service
                .GetServiceCategoriesByDepartmentIdAsync(2);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            category => Assert.Equal(
                2,
                category.DepartmentId));

        Assert.Equal("Road Damage", result[0].Name);
        Assert.Equal("Streetlight Issue", result[1].Name);

        Assert.Equal(
            2,
            _serviceCategoryRepository.LastDepartmentId);
    }

    [Fact]
    public async Task GetServiceCategoriesByDepartmentIdAsync_ShouldThrowNotFound_WhenDepartmentDoesNotExist()
    {
        // Arrange
        _departmentRepository.ExistsResult = false;

        // Act
        NotFoundException exception =
            await Assert.ThrowsAsync<NotFoundException>(
                () => _service
                    .GetServiceCategoriesByDepartmentIdAsync(
                        999));

        // Assert
        Assert.Contains("999", exception.Message);

        Assert.Equal(
            0,
            _serviceCategoryRepository.GetActiveCallCount);
    }

    [Fact]
    public async Task GetServiceCategoriesByDepartmentIdAsync_ShouldRejectNonPositiveId()
    {
        // Act
        ArgumentOutOfRangeException exception =
            await Assert.ThrowsAsync<
                ArgumentOutOfRangeException>(
                () => _service
                    .GetServiceCategoriesByDepartmentIdAsync(
                        0));

        // Assert
        Assert.Contains(
            "greater than zero",
            exception.Message);

        Assert.Equal(
            0,
            _departmentRepository.ExistsCallCount);

        Assert.Equal(
            0,
            _serviceCategoryRepository.GetActiveCallCount);
    }

    private sealed class FakeDepartmentRepository
        : IDepartmentRepository
    {
        public bool ExistsResult { get; set; } = true;

        public IReadOnlyList<Department> ActiveDepartments
        {
            get;
            set;
        } = [];

        public int ExistsCallCount { get; private set; }

        public int GetActiveCallCount { get; private set; }

        public Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            ExistsCallCount++;

            return Task.FromResult(ExistsResult);
        }

        public Task<IReadOnlyList<Department>> GetActiveAsync(
            CancellationToken cancellationToken = default)
        {
            GetActiveCallCount++;

            return Task.FromResult(ActiveDepartments);
        }
    }

    private sealed class FakeServiceCategoryRepository
        : IServiceCategoryRepository
    {
        public IReadOnlyList<ServiceCategory> ActiveCategories
        {
            get;
            set;
        } = [];

        public int? LastDepartmentId { get; private set; }

        public int GetActiveCallCount { get; private set; }

        public Task<bool> ExistsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<int?> GetDepartmentIdAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<int?>(2);
        }

        public Task<IReadOnlyList<ServiceCategory>>
            GetActiveByDepartmentIdAsync(
                int departmentId,
                CancellationToken cancellationToken = default)
        {
            LastDepartmentId = departmentId;
            GetActiveCallCount++;

            return Task.FromResult(ActiveCategories);
        }
    }
}