using EnergyTrade.Domain.Enums;
using MediatR;

namespace EnergyTrade.Application.Portfolios.Create;

public sealed record CreatePortfolioCommand(
    Guid UserId,
    string Name,
    Currency BaseCurrency)
    : IRequest<CreatePortfolioResult>
{
    public static CreatePortfolioCommand FromRequest(
        Guid userId,
        CreatePortfolioRequest request)
    {
        return new CreatePortfolioCommand(
            userId,
            request.Name,
            request.BaseCurrency);
    }
}
