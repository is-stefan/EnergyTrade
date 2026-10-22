using MediatR;

namespace EnergyTrade.Application.Auth.Register;

public sealed record RegisterCommand(
    string Email,
    string Password)
    : IRequest<RegisterResult>
{
    public static RegisterCommand FromRequest(RegisterRequest request)
    {
        return new RegisterCommand(
            request.Email,
            request.Password);
    }
}
