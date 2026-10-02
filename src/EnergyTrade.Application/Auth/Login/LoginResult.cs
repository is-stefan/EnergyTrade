namespace EnergyTrade.Application.Auth.Login;

public sealed record LoginResult(
    Guid UserId,
    string Email,
    string Token);