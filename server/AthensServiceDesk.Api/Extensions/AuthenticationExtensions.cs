using AthensServiceDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AthensServiceDesk.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>(
                (bearerOptions, jwtOptionsAccessor) =>
                {
                    JwtOptions jwtOptions = jwtOptionsAccessor.Value;

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
                                    Encoding.UTF8.GetBytes(jwtOptions.Key)),

                            ValidateLifetime = true,
                            RequireExpirationTime = true,
                            RequireSignedTokens = true,

                            ClockSkew = TimeSpan.FromSeconds(30),

                            NameClaimType = "name",
                            RoleClaimType = "role"
                        };
                });
        services.AddAuthorization();

        return services;
    }
}
