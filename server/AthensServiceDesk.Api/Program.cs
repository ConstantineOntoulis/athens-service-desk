using AthensServiceDesk.Api.ExceptionHandling;
using AthensServiceDesk.Api.Extensions;
using AthensServiceDesk.Application;
using AthensServiceDesk.Infrastructure;
using AthensServiceDesk.Infrastructure.Persistence.Seeding;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddJwtAuthentication();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await using AsyncServiceScope scope =
        app.Services.CreateAsyncScope();

    DatabaseInitializer databaseInitializer =
        scope.ServiceProvider
            .GetRequiredService<DatabaseInitializer>();

    await databaseInitializer.InitializeAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Athens Service Desk API");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}