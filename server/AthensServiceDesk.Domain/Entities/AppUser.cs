using AthensServiceDesk.Domain.Common;
using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Domain.Entities;

public class AppUser : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Citizen;
    public bool IsActive { get; set; } = true;
    public ICollection<ServiceRequest> CreatedRequests { get; set; } = [];
    public ICollection<ServiceRequest> AssignedRequests { get; set; } = [];
}
