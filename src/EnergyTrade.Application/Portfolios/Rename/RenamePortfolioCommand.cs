using MediatR;

namespace EnergyTrade.Application.Portfolios.Rename;

public sealed record RenamePortfolioCommand(
    Guid UserId,
    Guid PortfolioId,
    string Name)
    : IRequest<bool>
{
    public static RenamePortfolioCommand FromRequest(
        Guid userId,
        Guid portfolioId,
        RenamePortfolioRequest request)
    {
        return new RenamePortfolioCommand(
            userId,
            portfolioId,
            request.Name);
    }
}
