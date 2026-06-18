using AthensServiceDesk.Application.Interfaces.Persistence;
using AthensServiceDesk.Infrastructure.Persistence;
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

        return services;
    }
}