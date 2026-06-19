using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Domain.Entities;
using AthensServiceDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AthensServiceDesk.Infrastructure.Persistence.Seeding;

public sealed class DatabaseInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly DemoUserOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        AppDbContext dbContext,
        IPasswordService passwordService,
        IOptions<DemoUserOptions> options,
        TimeProvider timeProvider,
        ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        (string Email,
         string FirstName,
         string LastName,
         UserRole Role)[] demoUsers =
         [
             (
                "citizen@athensdesk.local",
                "Demo",
                "Citizen",
                UserRole.Citizen
             ),
             (
                "staff@athensdesk.local",
                "Demo",
                "Staff",
                UserRole.Staff
             ),
             (
                "manager@athensdesk.local",
                "Demo",
                "Manager",
                UserRole.Manager
             ),
             (
                "admin@athensdesk.local",
                "Demo",
                "Admin",
                UserRole.Admin
             )
         ];

        int addedUserCount = 0;

        foreach (var demoUser in demoUsers)
        {
            string normalizedEmail = demoUser.Email.ToUpperInvariant();

            bool alreadyExists =
                await _dbContext.Users.AnyAsync(
                    user =>
                        user.NormalizedEmail == normalizedEmail, cancellationToken);

            if (alreadyExists)
            {
                continue;
            }

            var user = new AppUser
            {
                Email = demoUser.Email,
                NormalizedEmail = normalizedEmail,
                FirstName = demoUser.FirstName,
                LastName = demoUser.LastName,
                Role = demoUser.Role,
                IsActive = true,
                CreatedAt = _timeProvider.GetUtcNow()
            };

            user.PasswordHash =
                _passwordService.HashPassword(
                    user,
                    _options.DefaultPassword);

            await _dbContext.Users.AddAsync(
                user,
                cancellationToken);

            addedUserCount++;
        }

        if (addedUserCount == 0)
        {
            return;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created {DemoUserCount} missing demo user accounts.", addedUserCount);
    }  
}
