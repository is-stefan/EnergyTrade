using MediatR;

namespace EnergyTrade.Application.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password)
    : IRequest<LoginResult>
{
    public static LoginCommand FromRequest(LoginRequest request)
    {
        return new LoginCommand(
            request.Email,
            request.Password);
    }
}
