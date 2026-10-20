using System.IdentityModel.Tokens.Jwt;
using EnergyTrade.Application.Abstractions.Authentication;
using EnergyTrade.Application.Common.Exceptions;

namespace EnergyTrade.Api.Authentication;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst(JwtRegisteredClaimNames.Sub)?
                .Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new UnauthorizedException(
                    "Authenticated user id was not found.");
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException(
                    "Authenticated user id is invalid.");
            }

            return userId;
        }
    }
}