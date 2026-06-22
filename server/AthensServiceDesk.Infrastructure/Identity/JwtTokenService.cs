using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Application.Interfaces.Security;
using AthensServiceDesk.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace AthensServiceDesk.Infrastructure.Identity;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly JsonWebTokenHandler _tokenHandler = new();

    public JwtTokenService(
        IOptions<JwtOptions> options,
        TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public AccessTokenResult CreateAccessToken(AppUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        DateTimeOffset issuedAt = _timeProvider.GetUtcNow();

        DateTimeOffset expiresAt = issuedAt.AddMinutes(_options.AccessTokenMinutes);

        Claim[] claims =
        [
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString(
                    CultureInfo.InvariantCulture)),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                JwtRegisteredClaimNames.Name,
                $"{user.FirstName} {user.LastName}".Trim()),

            new(
                JwtRegisteredClaimNames.GivenName,
                user.FirstName),

            new(
                JwtRegisteredClaimNames.FamilyName,
                user.LastName),

            new(
                "role",
                user.Role.ToString()),

            new(
                "client_id",
                _options.ClientId),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        ];

        var signingKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _options.Key));

        var tokenDescriptor =
            new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                IssuedAt = issuedAt.UtcDateTime,
                NotBefore = issuedAt.UtcDateTime,
                Expires = expiresAt.UtcDateTime,
                SigningCredentials =
                    new SigningCredentials(
                        signingKey,
                        SecurityAlgorithms.HmacSha256)
            };

        string token = _tokenHandler.CreateToken(tokenDescriptor);

        return new AccessTokenResult(token, expiresAt);
    }
}
