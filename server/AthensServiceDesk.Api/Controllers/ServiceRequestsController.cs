using AthensServiceDesk.Api.Authorization;
using AthensServiceDesk.Application.DTOs.Common;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AthensServiceDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/service-requests")]
public sealed class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;

    public ServiceRequestsController(
        IServiceRequestService serviceRequestService)
    {
        _serviceRequestService = serviceRequestService;
    }

    [HttpGet]
    [ProducesResponseType<
        PagedResponse<ServiceRequestResponse>>(
            StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<PagedResponse<ServiceRequestResponse>>> GetPaged(
        [FromQuery] ServiceRequestQuery query,
        CancellationToken cancellationToken)
    {
        PagedResponse<ServiceRequestResponse> response =
            await _serviceRequestService.GetPagedAsync(
                query,
                cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ServiceRequestDetailsResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<ServiceRequestDetailsResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        ServiceRequestDetailsResponse response =
            await _serviceRequestService.GetByIdAsync(
                id,
                cancellationToken);

        return Ok(response);
    }

    [Authorize(
        Policy = AuthorizationPolicies.CitizenOnly)]
    [HttpPost]
    [ProducesResponseType<ServiceRequestDetailsResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<
        ActionResult<ServiceRequestDetailsResponse>> Create(
        [FromBody] CreateServiceRequestRequest request,
        CancellationToken cancellationToken)
    {
        ServiceRequestDetailsResponse response =
            await _serviceRequestService.CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [Authorize(
        Policy = AuthorizationPolicies.CitizenOnly)]
    [HttpPut("{id:int}")]
    [ProducesResponseType<ServiceRequestDetailsResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<
        ActionResult<ServiceRequestDetailsResponse>> Update(
        int id,
        [FromBody] UpdateServiceRequestRequest request,
        CancellationToken cancellationToken)
    {
        ServiceRequestDetailsResponse response =
            await _serviceRequestService.UpdateAsync(
                id,
                request,
                cancellationToken);

        return Ok(response);
    }
}