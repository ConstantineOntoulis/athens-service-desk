using System.ComponentModel.DataAnnotations;

namespace AthensServiceDesk.Application.DTOs.ServiceRequests;

public sealed class AssignServiceRequestRequest
{
    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "The staff user identifier must be greater than zero.")]
    public int StaffUserId { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}