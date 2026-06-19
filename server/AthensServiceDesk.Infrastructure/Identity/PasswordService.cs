using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AthensServiceDesk.Infrastructure.Identity;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public string HashPassword(
        AppUser user,
        string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(
        AppUser user,
        string passwordHash,
        string prividedPassword)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(prividedPassword);

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(
            user,
            passwordHash,
            prividedPassword);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
