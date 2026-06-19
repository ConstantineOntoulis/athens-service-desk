using AthensServiceDesk.Domain.Entities;

namespace AthensServiceDesk.Application.Interfaces.Security;

public interface IPasswordService
{
    string HashPassword(
        AppUser user,
        string password);

    bool VerifyPassword(
        AppUser user,
        string passwordHash,
        string providedPassword);
}
