using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Application.Common.Exceptions;
using MediatR;

namespace EnergyTrade.Application.Portfolios.Rename;

public sealed class RenamePortfolioCommandHandler
    : IRequestHandler<RenamePortfolioCommand, bool>
{
    private readonly IPortfolioRepository _portfolioRepository;

    public RenamePortfolioCommandHandler(
        IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<bool> Handle(
        RenamePortfolioCommand command,
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

        portfolio.Rename(command.Name);

        await _portfolioRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
