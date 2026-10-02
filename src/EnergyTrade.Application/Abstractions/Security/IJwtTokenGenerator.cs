namespace EnergyTrade.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string Generate(
        Guid userId,
        string email);
}