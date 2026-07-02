using System.ComponentModel.DataAnnotations;

namespace AthensServiceDesk.Application.DTOs.ServiceRequests;

public sealed class ResolveServiceRequestRequest
{
    [Required]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "The resolution note must contain between 10 and 500 characters.")]

    public string ResolutionNote { get; set; } = string.Empty;
}
