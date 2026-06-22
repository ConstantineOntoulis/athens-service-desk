using AthensServiceDesk.Domain.Enums;

namespace AthensServiceDesk.Application.Interfaces.Security;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    int? UserId { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
    UserRole? Role { get; }
}
