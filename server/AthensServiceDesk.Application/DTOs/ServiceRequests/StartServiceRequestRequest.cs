using System.ComponentModel.DataAnnotations;

namespace AthensServiceDesk.Application.DTOs.ServiceRequests;

public sealed class StartServiceRequestRequest
{
    [StringLength(500, ErrorMessage = "The workflow not cannot exceed 500 characters.")]

    public string? Note { get; set; }
}
