using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;
using MediatR;

namespace EnergyTrade.Application.Portfolios.Create;

public sealed class CreatePortfolioCommandHandler
    : IRequestHandler<CreatePortfolioCommand, CreatePortfolioResult>
{
    private readonly IPortfolioRepository _portfolioRepository;

    public CreatePortfolioCommandHandler(
        IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<CreatePortfolioResult> Handle(
        CreatePortfolioCommand command,
        CancellationToken cancellationToken)
    {
        var portfolio = new Portfolio(
            command.UserId,
            command.Name,
            command.BaseCurrency);

        await _portfolioRepository.AddAsync(
            portfolio,
            cancellationToken);

        await _portfolioRepository.SaveChangesAsync(
            cancellationToken);

        return new CreatePortfolioResult(
            portfolio.Id,
            portfolio.UserId,
            portfolio.Name,
            portfolio.BaseCurrency,
            portfolio.Status,
            portfolio.CreatedAt);
    }
}
