using AthensServiceDesk.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AthensServiceDesk.IntegrationTests;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

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