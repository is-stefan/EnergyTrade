using EnergyTrade.Application.Abstractions.Persistence;
using EnergyTrade.Domain.Entities;

namespace EnergyTrade.Application.Positions.ApplyTrade;

public sealed class ApplyTradeToPositionsService
{
    private readonly IPositionRepository _positionRepository;

    public ApplyTradeToPositionsService(
        IPositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task ExecuteAsync(
        Trade trade,
        Guid buyerPortfolioId,
        Guid sellerPortfolioId,
        CancellationToken cancellationToken = default)
    {
        var buyerPosition =
            await _positionRepository.GetByPortfolioAndEnergyTypeAsync(
                buyerPortfolioId,
                trade.EnergyType,
                cancellationToken);

        if (buyerPosition is null)
        {
            buyerPosition = new Position(
                buyerPortfolioId,
                trade.EnergyType);

            await _positionRepository.AddAsync(
                buyerPosition,
                cancellationToken);
        }

        var sellerPosition =
            await _positionRepository.GetByPortfolioAndEnergyTypeAsync(
                sellerPortfolioId,
                trade.EnergyType,
                cancellationToken);

        if (sellerPosition is null)
        {
            sellerPosition = new Position(
                sellerPortfolioId,
                trade.EnergyType);

            await _positionRepository.AddAsync(
                sellerPosition,
                cancellationToken);
        }

        buyerPosition.ApplyTrade(trade.QuantityMWh);
        sellerPosition.ApplyTrade(-trade.QuantityMWh);
    }
}