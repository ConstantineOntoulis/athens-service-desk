using System.Text;
using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Infrastructure.Identity;
using AthensServiceDesk.Infrastructure.Persistence;
using AthensServiceDesk.Infrastructure.Persistence.Seeding;
using AthensServiceDesk.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AthensServiceDesk.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            string connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection was not found.");

            options.UseSqlServer(connectionString);
        });

        services.AddOptions<JwtOptions>().Bind(
            configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "Jwt:ClientId is required.")
            .Validate(options => Encoding.UTF8.GetByteCount(options.Key) >= 32, "Jwt: Key must contain at least 32 bytes.")
            .Validate(options => options.AccessTokenMinutes is >= 5 and <= 1440, "Jwt:AccessTokenMinutes must be between 5 and 1440.")
            .ValidateOnStart();

        services.AddOptions<DemoUserOptions>().Bind(configuration.GetSection(DemoUserOptions.SectionName))
            .Validate(options => !options.Enabled || options.DefaultPassword.Length >= 12, "The demo-user password must contain at least 12 characters when demo-user seeding is enabled.")
            .ValidateOnStart();

        services.AddSingleton<TimeProvider>(TimeProvider.System);

        services.AddScoped<IPasswordService, PasswordService>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<
            IDepartmentRepository,
            DepartmentRepository>();

        services.AddScoped<
            IServiceCategoryRepository,
            ServiceCategoryRepository>();

        services.AddScoped<
            IServiceRequestRepository,
            ServiceRequestRepository>();

        services.AddScoped<
            IUserRepository,
            UserRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<DatabaseInitializer>();

        return services;
    }
}