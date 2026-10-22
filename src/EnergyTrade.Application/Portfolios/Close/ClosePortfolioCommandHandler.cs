using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.Portfolios.Close;

public sealed class ClosePortfolioCommandHandler
    : IRequestHandler<ClosePortfolioCommand, bool>
{
    private readonly IPortfolioRepository _portfolioRepository;

    public ClosePortfolioCommandHandler(
        IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<bool> Handle(
        ClosePortfolioCommand command,
        CancellationToken cancellationToken)
    {
        var portfolio = await _portfolioRepository.GetByIdAsync(
            command.PortfolioId,
            cancellationToken);

        if (portfolio is null)
        {
            return false;
        }

        if (portfolio.UserId != command.UserId)
        {
            throw new ForbiddenException(
                "The portfolio does not belong to the authenticated user.");
        }

        portfolio.Close();

        await _portfolioRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
