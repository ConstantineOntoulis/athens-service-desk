using AthensServiceDesk.Api.Authorization;
using AthensServiceDesk.Api.Security;
using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Domain.Enums;
using AthensServiceDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AthensServiceDesk.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>(
                (bearerOptions, jwtOptionsAccessor) =>
                {
                    JwtOptions jwtOptions =
                        jwtOptionsAccessor.Value;

                    bearerOptions.MapInboundClaims = false;

                    bearerOptions.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = jwtOptions.Issuer,

                            ValidateAudience = true,
                            ValidAudience = jwtOptions.Audience,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwtOptions.Key)),

                            ValidateLifetime = true,
                            RequireExpirationTime = true,
                            RequireSignedTokens = true,

                            ClockSkew =
                                TimeSpan.FromSeconds(30),

                            NameClaimType = "name",
                            RoleClaimType = "role"
                        };
                });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.CitizenOnly,
                policy =>
                {
                    policy.RequireAuthenticatedUser();

                    policy.RequireRole(
                        nameof(UserRole.Citizen));
                });
        });

        services.AddSingleton<
            IAuthorizationMiddlewareResultHandler,
            ProblemDetailsAuthorizationMiddlewareResultHandler>();

        services.AddHttpContextAccessor();

        services.AddScoped<
            ICurrentUserService,
            HttpCurrentUserService>();

        return services;
    }
}