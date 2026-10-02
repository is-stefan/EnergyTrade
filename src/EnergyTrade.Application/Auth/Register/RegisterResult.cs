namespace EnergyTrade.Application.Auth.Register;

public sealed record RegisterResult(
    Guid Id,
    string Email,
    DateTimeOffset CreatedAt);