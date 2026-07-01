using AthensServiceDesk.Application.Interfaces.Services;
using AthensServiceDesk.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AthensServiceDesk.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<
            IServiceRequestService,
            ServiceRequestService>();

        services.AddScoped<
            IServiceRequestWorkflowService,
            ServiceRequestWorkflowService>();

        services.AddScoped<
            IServiceCatalogService,
            ServiceCatalogService>();

        services.AddScoped<
            IAuthService,
            AuthService>();

        return services;
    }
}