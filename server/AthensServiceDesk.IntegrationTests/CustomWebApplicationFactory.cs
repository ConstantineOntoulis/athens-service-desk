using AthensServiceDesk.Infrastructure.Persistence;
using AthensServiceDesk.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AthensServiceDesk.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string DemoPassword = "AthensTest!2026";

    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "AthensServiceDesk.Tests",
                ["Jwt:Audience"] = "AthensServiceDesk.TeskClient",
                ["Jwt:ClientId"] = "athens-service-desk-tests",
                ["Jwt:Key"] = "integration-test-signing-key-that-is-longer-than-thirty-two-bytes",
                ["Jwt:AccessTokenMinutes"] = "60",
                ["DemoUsers:Enabled"] = "true",
                ["DemoUsers:DefaultPassword"] = DemoPassword
            };

            configuration.AddInMemoryCollection(settings);

        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                DbContextOptions<AppDbContext>>();

            services.RemoveAll<
                IDbContextOptionsConfiguration<AppDbContext>>();

            services.RemoveAll<AppDbContext>();

            _connection = new SqliteConnection(
                "Data Source=:memory:");

            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(
        IHostBuilder builder)
    {
        IHost host = base.CreateHost(builder);

        using IServiceScope scope =
            host.Services.CreateScope();

        AppDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

        dbContext.Database.EnsureCreated();

        DatabaseInitializer initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();

        initializer.InitializeAsync().GetAwaiter().GetResult();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }

        base.Dispose(disposing);
    }
}