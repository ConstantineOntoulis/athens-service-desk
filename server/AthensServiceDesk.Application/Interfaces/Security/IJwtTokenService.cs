
using AthensServiceDesk.Application.Common.Models;
using AthensServiceDesk.Domain.Entities;

namespace AthensServiceDesk.Application.Interfaces.Security;

public interface IJwtTokenService
{
    AccessTokenResult CreateAccessToken(AppUser user);
}
