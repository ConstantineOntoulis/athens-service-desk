using AthensServiceDesk.Application.DTOs.Common;
using AthensServiceDesk.Application.DTOs.ServiceRequests;
using AthensServiceDesk.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AthensServiceDesk.Api.Controllers;

[ApiController]
[Route("api/service-requests")]
public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;

public ServiceRequestsController(
    IServiceRequestService serviceRequestService)
    {
        _serviceRequestService = serviceRequestService;
    }

    [HttpGet]
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
    public async Task<ActionResult<ServiceRequestDetailsResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        ServiceRequestDetailsResponse response =
            await _serviceRequestService.GetByIdAsync(
                id,
                cancellationToken);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceRequestDetailsResponse>> Create(
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

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceRequestDetailsResponse>> Update(
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
