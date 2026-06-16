using AthensServiceDesk.Application.DTOs.Lookups;
using AthensServiceDesk.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AthensServiceDesk.Api.Controllers;

[ApiController]
[Route("api/departments")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IServiceCatalogService _serviceCatalogService;

    public DepartmentsController(
        IServiceCatalogService serviceCatalogService)
    {
        _serviceCatalogService = serviceCatalogService;
    }
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<DepartmentResponse>>> GetDepartments(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<DepartmentResponse> response =
            await _serviceCatalogService.GetDepartmentsAsync(
                cancellationToken);
        return Ok(response);
    }

    [HttpGet(
        "{departmentId:int:min(1)}/service-categories")]
    public async Task<
        ActionResult<IReadOnlyList<ServiceCategoryResponse>>>
        GetServiceCategories(
            int departmentId,
            CancellationToken cancellationToken)
    {
        IReadOnlyList<ServiceCategoryResponse> response =
            await _serviceCatalogService
                .GetServiceCategoriesByDepartmentIdAsync(
                    departmentId,
                    cancellationToken);
        return Ok(response);
    }
}
