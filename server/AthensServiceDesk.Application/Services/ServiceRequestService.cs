using AthensServiceDesk.Application.Common.Exceptions;
using AthensServiceDesk.Application.DTOs.Common;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Application.Interfaces.Services;
using AthensServiceDesk.Application.Mappings;
using AthensServiceDesk.Application.Rules;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Application.Services;

public sealed class ServiceRequestService : IServiceRequestService
{
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IServiceCategoryRepository _serviceCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceRequestService(
        IServiceRequestRepository serviceRequestRepository,
        IDepartmentRepository departmentRepository,
        IServiceCategoryRepository serviceCategoryRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _departmentRepository = departmentRepository;
        _serviceCategoryRepository = serviceCategoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<ServiceRequestResponse>> GetPagedAsync(
        ServiceRequestQuery query,
        CancellationToken cancellationToken = default)
    {
        ServiceRequestQuery normalizedQuery =
            QueryRules.Normalize(query);

        IReadOnlyList<ServiceRequest> serviceRequests =
            await _serviceRequestRepository.ListAsync(
                normalizedQuery,
                cancellationToken);

        int totalCount =
            await _serviceRequestRepository.CountAsync(
                normalizedQuery,
                cancellationToken);

        List<ServiceRequestResponse> responseItems = serviceRequests
            .Select(ServiceRequestMapper.ToResponse)
            .ToList();

        return new PagedResponse<ServiceRequestResponse>
        {
            Items = responseItems,
            Page = normalizedQuery.Page,
            PageSize = normalizedQuery.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ServiceRequestDetailsResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);

        ServiceRequest? serviceRequest =
            await _serviceRequestRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (serviceRequest is null)
        {
            throw new NotFoundException("Service request", id);
        }

        return ServiceRequestMapper.ToDetailsResponse(serviceRequest);
    }

    public async Task<ServiceRequestDetailsResponse> CreateAsync(
        CreateServiceRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await ValidateDepartmentAndCategoryAsync(
            request.DepartmentId,
            request.ServiceCategoryId,
            cancellationToken);

        var serviceRequest = new ServiceRequest
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            DepartmentId = request.DepartmentId,
            ServiceCategoryId = request.ServiceCategoryId,
            Priority = request.Priority,

            // The client is not allowed to choose the initial status.
            Status = ServiceRequestStatus.Submitted,

            // This will be assigned from the authenticated user later.
            CreatedByUserId = null,

            CreatedAt = DateTimeOffset.UtcNow
        };

        await _serviceRequestRepository.AddAsync(
            serviceRequest,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        ServiceRequest? createdServiceRequest =
            await _serviceRequestRepository.GetByIdAsync(
                serviceRequest.Id,
                cancellationToken);

        if (createdServiceRequest is null)
        {
            throw new NotFoundException(
                "Newly created service request",
                serviceRequest.Id);
        }

        return ServiceRequestMapper.ToDetailsResponse(
            createdServiceRequest);
    }

    public async Task<ServiceRequestDetailsResponse> UpdateAsync(
        int id,
        UpdateServiceRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureValidId(id);
        ArgumentNullException.ThrowIfNull(request);

        ServiceRequest? serviceRequest =
            await _serviceRequestRepository.GetByIdForUpdateAsync(
                id,
                cancellationToken);

        if (serviceRequest is null)
        {
            throw new NotFoundException("Service request", id);
        }

        if (!ServiceRequestRules.CanEdit(serviceRequest.Status))
        {
            throw new BusinessRuleException(
                $"A service request with status " +
                $"'{serviceRequest.Status}' cannot be edited.");
        }

        await ValidateDepartmentAndCategoryAsync(
            request.DepartmentId,
            request.ServiceCategoryId,
            cancellationToken);

        serviceRequest.Title = request.Title.Trim();
        serviceRequest.Description = request.Description.Trim();
        serviceRequest.Location = request.Location.Trim();
        serviceRequest.DepartmentId = request.DepartmentId;
        serviceRequest.ServiceCategoryId = request.ServiceCategoryId;
        serviceRequest.Priority = request.Priority;
        serviceRequest.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

    private async Task ValidateDepartmentAndCategoryAsync(
        int departmentId,
        int serviceCategoryId,
        CancellationToken cancellationToken)
    {
        bool departmentExists =
            await _departmentRepository.ExistsAsync(
                departmentId,
                cancellationToken);

        if (!departmentExists)
        {
            throw new BusinessRuleException(
                "The selected department does not exist or is inactive.");
        }

        int? categoryDepartmentId =
            await _serviceCategoryRepository.GetDepartmentIdAsync(
                serviceCategoryId,
                cancellationToken);

        if (categoryDepartmentId is null)
        {
            throw new BusinessRuleException(
                "The selected service category does not exist or is inactive.");
        }

        bool categoryIsValid =
            ServiceRequestRules.IsCategoryValidForDepartment(
                categoryDepartmentId.Value,
                departmentId);

        if (!categoryIsValid)
        {
            throw new BusinessRuleException(
                "The selected service category does not belong " +
                "to the selected department.");
        }
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